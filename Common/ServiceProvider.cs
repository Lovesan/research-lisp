// $Id: ServiceProvider.cs 2421 2006-09-19 07:26:55Z kostya $

using System;
using System.Collections;
using System.ComponentModel.Design;
using System.Runtime.Serialization;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Lifetime;
using System.Runtime.Remoting;

using Front.Globalization;

namespace Front {
	/// <summary>���������, ������� ��������� ��������� ��������� ��������� ������� � ���������� ��������.</summary>
	public interface ISupportReplaceFactoryPolicy : IServiceContainer {
		/// <summary>Adds the specified service to the service container.</summary>
		/// <param name="serviceType">The type of service to add.</param>
		/// <param name="callback">A callback object that can create the service. This allows
		/// a service to be declared as available, but delays creation of the object until the
		/// service is requested.</param>
		/// <param name="promote">true if this service should be added to any parent service
		/// containers; otherwise, false.</param>
		/// <param name="replaceFactory">�������� ������ ������� ������� �� ���������� ������.</param>
		/// <remarks>���� <paramref name="replaceFactory"/> ����� true, �� ��� �������� ���������� �������
		/// � ������� �������� <paramref name="callback"/>, ������� ����� �������� �������� ����������� �������.
		/// ��� ��������, ��� ����������� ����������� ������� <see cref="IServiceProvider.GetService"/>, ����� ��� ��
		/// ��������� �������. � �������� ������, ������ ����� <see cref="IServiceProvider.GetService"/> ����� ��������� � ������
		/// �������� � � �������� ������ ���������� �������.</remarks>
		void AddService(Type serviceType, ServiceCreatorCallback callback, bool promote, bool replaceFactory);
	}
	
	/// <summary>Provides a simple implementation of the <see cref="IServiceContainer"/> interface.</summary>
	/// <remarks>The ServiceContainer object can be used to store and provide services. ServiceContainer
	/// implements the IServiceContainer interface.
	/// <para>The <see cref="ServiceContainer"/> object can be created using a constructor that adds a parent
	/// <see cref="IServiceContainer"/> through which services can be optionally added to or removed from all parent
	/// <see cref="IServiceContainer"/> objects, including the immediate parent <see cref="IServiceContainer"/>. To add or remove a
	/// service from all <see cref="IServiceContainer"/> implementations that are linked to this <see cref="IServiceContainer"/>
	/// through parenting, call the AddService or RemoveService method overload that accepts a Boolean
	/// value indicating whether to promote the service request.</para></remarks>
	public class ServiceContainer : MarshalByRefObject,
					IServiceContainer,
					IServiceProvider,
					ISupportReplaceFactoryPolicy,
					IDisposable
	{
		static Type[] defaultServices = new Type[] { typeof(IServiceContainer), typeof(ServiceContainer) };
		Hashtable                  services;
		protected IServiceProvider parentProvider;
		protected bool				   denyPromoting;
		protected bool	            defaultReplaceFactory = true;
		protected ArrayList        blockFactoryReplacing = new ArrayList();

		/// <summary>Initializes a new instance of the <see cref="ServiceContainer"/> class.</summary>
		public ServiceContainer():this(null) {
			services = new Hashtable();
		}

		/// <summary>Initializes a new instance of the <see cref="ServiceContainer"/> class.</summary>
		/// <param name="parent">A parent service provider.</param>
		public ServiceContainer(IServiceProvider parent):this(parent, false) { }

		/// <summary>Initializes a new instance of the <see cref="ServiceContainer"/> class.</summary>
		/// <param name="parent">A parent service provider.</param>
		/// <param name="denyPromoting">��������� ��� ��������� ���������� �������� � ������������
		/// <see cref="IServiceProvider"/>. ��� �������� true, ������ ��������� �� ����� ���������
		/// ���������� ������������� ����������.</param>
		public ServiceContainer(IServiceProvider parent, bool denyPromoting) {
			this.parentProvider = parent;
			this.denyPromoting = denyPromoting;
		}

		/// <summary>Initializes a new instance of the <see cref="ServiceContainer"/> class.</summary>
		/// <param name="parent">A parent service provider.</param>
		/// <param name="denyPromoting">��������� ��� ��������� ���������� �������� � ������������
		/// <see cref="IServiceProvider"/>. ��� �������� true, ������ ��������� �� ����� ���������
		/// ���������� ������������� ����������.</param>
		/// <param name="defaultReplaceFactory">�������� ��������� ������ ����������� ����������.</param>
		/// <remarks>���� <paramref name="defaultReplaceFactory"/> ����� true, �� ��� �������� ���������� �������
		/// � ������� �������� <paramref name="callback"/>, ������� ����� �������� �������� ����������� �������.
		/// ��� ��������, ��� ����������� ����������� ������� <see cref="IServiceProvider.GetService"/>, ����� ��� ��
		/// ��������� �������. � �������� ������, ������ ����� <see cref="IServiceProvider.GetService"/> ����� ��������� � ������
		/// �������� � � �������� ������ ���������� �������.</remarks>
		public ServiceContainer(IServiceProvider parent, bool denyPromoting, bool defaultReplaceFactory)
			:this(parent, denyPromoting) {
				this.defaultReplaceFactory = defaultReplaceFactory;
		}

		/// <summary>���������� ������������� ������� � <see cref="IDisposable"/> �������.</summary>
		public virtual void Dispose() {
		}

		/// <summary>Adds the specified service to the service container.</summary>
		/// <param name="serviceType">The type of service to add.</param>
		/// <param name="callback">A callback object that can create the service. This allows
		/// a service to be declared as available, but delays creation of the object until the
		/// service is requested.</param>
		public void AddService(Type serviceType, ServiceCreatorCallback callback) {
			this.AddService(serviceType, callback, false);
		}

		/// <summary>Adds the specified service to the service container.</summary>
		/// <param name="serviceType">The type of service to add.</param>
		/// <param name="instance">An instance of the service to add. This object must
		/// implement or inherit from the type indicated by the serviceType parameter.</param>
		public void AddService(Type serviceType, object instance) {
			this.AddService(serviceType, instance, false);
		}
		
		/// <summary>Adds the specified service to the service container.</summary>
		/// <param name="serviceType">The type of service to add.</param>
		/// <param name="callback">A callback object that can create the service. This allows
		/// a service to be declared as available, but delays creation of the object until the
		/// service is requested.</param>
		/// <param name="promote">true if this service should be added to any parent service
		/// containers; otherwise, false.</param>
		public void AddService(Type serviceType, ServiceCreatorCallback callback, bool promote) {
			this.AddService(serviceType, callback, promote, defaultReplaceFactory);
		}

		/// <summary>Adds the specified service to the service container.</summary>
		/// <param name="serviceType">The type of service to add.</param>
		/// <param name="callback">A callback object that can create the service. This allows
		/// a service to be declared as available, but delays creation of the object until the
		/// service is requested.</param>
		/// <param name="promote">true if this service should be added to any parent service
		/// containers; otherwise, false.</param>
		/// <param name="replaceFactory">�������� ������ ������� ������� �� ���������� ������.</param>
		/// <remarks>���� <paramref name="replaceFactory"/> ����� true, �� ��� �������� ���������� �������
		/// � ������� �������� <paramref name="callback"/>, ������� ����� �������� �������� ����������� �������.
		/// ��� ��������, ��� ����������� ����������� ������� <see cref="GetService"/>, ����� ��� ��
		/// ��������� �������. � �������� ������, ������ ����� <see cref="GetService"/> ����� ��������� � ������
		/// �������� � � �������� ������ ���������� �������.</remarks>
		public void AddService(Type serviceType, ServiceCreatorCallback callback, bool promote, bool replaceFactory) {
			if (promote && !denyPromoting) {
				IServiceContainer container = this.Container;
				if (container != null) {
					ISupportReplaceFactoryPolicy srfp = container as ISupportReplaceFactoryPolicy;
					if (srfp != null)
						srfp.AddService(serviceType, callback, true, replaceFactory);
					else
						container.AddService(serviceType, callback, true);
					return;
				}
			}
			if (serviceType == null) throw new ArgumentNullException("serviceType");				
			if (callback == null) throw new ArgumentNullException("callback");
			if (Services.ContainsKey(serviceType))
				throw new ArgumentException(RM.GetString("ErrServiceExists", serviceType.FullName), "serviceType");

			OnRegisterService(new ServiceContainerEventArgs(serviceType, callback, replaceFactory));

			this.Services[serviceType] = callback;
			if (!replaceFactory) BlockFactoryReplace(serviceType);
		}

        /// <summary>��������� ������ ������� �������� �� ��� ������ ������������ ��������
		/// <see cref="GetService"/> ��� ���������� ����.</summary>
		/// <param name="serviceType">��� �������, ��� �������� ����������� ������.</param>
		protected void BlockFactoryReplace(Type serviceType) {
			if (blockFactoryReplacing == null) blockFactoryReplacing = new ArrayList();
			if (!blockFactoryReplacing.Contains(serviceType))
				blockFactoryReplacing.Add(serviceType);
		}

		
		/// <summary>������� ���������� ������ ������� �������� �� ��� ������ ������������ ��������
		/// <see cref="GetService"/> ��� ���������� ����.</summary>
		/// <param name="serviceType">��� �������, ��� �������� ����������� ������.</param>
		protected void UnblockFactoryReplace(Type serviceType) {
			if (blockFactoryReplacing != null)
				blockFactoryReplacing.Remove(serviceType);
		}

		/// <summary>Adds the specified service to the service container.</summary>
		/// <param name="serviceType">The type of service to add.</param>
		/// <param name="instance">An instance of the service to add. This object must
		/// implement or inherit from the type indicated by the serviceType parameter.</param>
		/// <param name="promote">true if this service should be added to any parent service
		/// containers; otherwise, false.</param>
		public void AddService(Type serviceType, object instance, bool promote) {
			if (promote && !denyPromoting) {
				IServiceContainer container = this.Container;
				if (container != null) {
					container.AddService(serviceType, instance, true);
					return;
				}
			}
			if (serviceType == null) throw new ArgumentNullException("serviceType");				
			if (instance == null) throw new ArgumentNullException("instance");
			if (Services.ContainsKey(serviceType))
				throw new ArgumentException(RM.GetString("ErrServiceExists", serviceType.FullName), "serviceType");

			OnRegisterService(new ServiceContainerEventArgs(serviceType, instance, true));
			this.Services[serviceType] = instance;
		}

		/// <summary>Replaces the specified service in the service container.</summary>
        /// <param name="serviceType">The type of service to replace.</param>
        /// <param name="instance">An instance of the service to replace. This object must
        /// implement or inherit from the type indicated by the serviceType parameter.</param>
        public void ReplaceService(Type serviceType, object instance)
        {
            object service = this.Services[serviceType];
            if (service == null)
            {
                AddService(serviceType, instance);
            }
            else
            {
                RemoveService(serviceType);
                AddService(serviceType, instance);
            }
        }

        protected virtual void OnRegisterService(ServiceContainerEventArgs e) {
			if (!IsServiceValid(e.ServiceType, e.Instance))
				throw new ArgumentException(RM.GetString("ErrInvalidServiceInstance", e.ServiceType.FullName));
			ServiceContainerEventHandler h = this.RegisterService;
			if (h != null) h(this, e);
		}

		protected virtual void OnUnregisterService(ServiceContainerEventArgs e) {
			ServiceContainerEventHandler h = this.UnregisterService;
			if (h != null) h(this, e);
		}

		public event ServiceContainerEventHandler RegisterService;
		public event ServiceContainerEventHandler UnregisterService;

		protected virtual bool IsServiceValid(Type serviceType, object instance) {
			if (instance == null) return false;

			// TODO DF0025: ����� ����� ����������� � ������ � �����������
			// ���� 2 ������, ����� �� �� ��������� �������������� ������� �
			// ��������������� ����: COM � Remoting
//			Type instType = instance.GetType();
//			if (instType.IsCOMObject || System.Runtime.Remoting.RemotingServices.IsTransparentProxy(instance))
				return true;

//			return (instance is ServiceCreatorCallback) || serviceType.IsAssignableFrom(instType);
		}

		/// <summary>Removes the specified service type from the service container.</summary>
		/// <param name="serviceType">The type of service to remove.</param>
		public void RemoveService(Type serviceType) {
			this.RemoveService(serviceType, false);
		}

		/// <summary>Removes the specified service type from the service container.</summary>
		/// <param name="serviceType">The type of service to remove.</param>
		/// <param name="promote">true if this service should be removed from any parent service
		/// containers; otherwise, false.</param>
		public void RemoveService(Type serviceType, bool promote) {
			if (promote && !denyPromoting) {
				IServiceContainer container = this.Container;
				if (container != null) {
					container.RemoveService(serviceType, true);
					return;
				}
			}
			if (serviceType == null) throw new ArgumentNullException("serviceType");				
			object instance = this.Services[serviceType]; 
			if (instance != null)
				OnUnregisterService(new ServiceContainerEventArgs(serviceType, instance, true));
			this.Services.Remove(serviceType);
		}

		/// <summary>Gets the requested service.</summary>
		/// <value>An instance of the service if it could be found, or a null reference
		/// (Nothing in Visual Basic) if it could not be found.</value>
		/// <param name="serviceType">The type of service to retrieve.</param>
		public object GetService(Type serviceType) {
			object result = null;
			foreach (Type type in DefaultServices)
				if (type == serviceType) {
					result = this;
					break;
				}
			if (result == null) result = this.Services[serviceType];

			if (result != null && result is ServiceCreatorCallback) {
				result = ((ServiceCreatorCallback)result)(this, serviceType);
				if (!blockFactoryReplacing.Contains(serviceType)) {
					OnRegisterService(new ServiceContainerEventArgs(serviceType, result, true));
					this.Services[serviceType] = result;
				}
			}
			if (result == null && parentProvider != null)
				result = parentProvider.GetService(serviceType);
			return result;
		}

		/// <summary>����� �����, ������� <see cref="ServiceContainer"/> ������� ������,
		/// �� ������� �� �������� �����.</summary>
		protected virtual Type[] DefaultServices { get { return defaultServices; } }

		protected Hashtable Services {
			get {
				if (services == null) services = new Hashtable();
				return services;
			}
		}

		protected IServiceContainer Container {
			get {
				IServiceContainer container = null;
				if (parentProvider != null)
					container = (IServiceContainer) this.parentProvider.GetService(typeof(IServiceContainer));
				return container;
			}
		}

		public virtual bool Contains( Type serviceType ) {
			foreach (Type type in DefaultServices)
				if (type == serviceType)
					return true;
			
			if (this.Services.ContainsKey(serviceType)) return true;
			if (this.parentProvider != null) {
				ServiceContainer cs = parentProvider as ServiceContainer;
				if (cs != null) 
					return cs.Contains(serviceType);
				else
					// XXX ��� ������� ���������������� ����������
					return (parentProvider.GetService(serviceType) != null);
			}
			return false;
		}
	}

	[Serializable]
	public class ServiceContainerEventArgs : EventArgs {
		public Type		ServiceType;
		public object	Instance;
		public bool		ReplaceFactory;
		public ServiceContainerEventArgs(Type serviceType, object instance, bool replaceFactory) {
			this.ServiceType = serviceType;
			this.Instance = instance;
			this.ReplaceFactory = replaceFactory;
		}
	}

	public delegate void ServiceContainerEventHandler(object sender, ServiceContainerEventArgs e);

	public class RemotableServiceContainer : ServiceContainer, ISponsor {
		// ����� ��������� ��������
		TimeSpan leaseTime = TimeSpan.FromMinutes(1000); //XXX

		/// <summary>Initializes a new instance of the <see cref="RemotableServiceContainer"/> class.</summary>
		public RemotableServiceContainer():base() { }

		/// <summary>Initializes a new instance of the <see cref="RemotableServiceContainer"/> class.</summary>
		/// <param name="parent">A parent service provider.</param>
		public RemotableServiceContainer(IServiceProvider parent):this(parent, false) { }

		/// <summary>Initializes a new instance of the <see cref="RemotableServiceContainer"/> class.</summary>
		/// <param name="parent">A parent service provider.</param>
		/// <param name="denyPromoting">��������� ��� ��������� ���������� �������� � ������������
		/// <see cref="IServiceProvider"/>. ��� �������� true, ������ ��������� �� ����� ���������
		/// ���������� ������������� ����������.</param>
		public RemotableServiceContainer(IServiceProvider parent, bool denyPromoting):base(parent, denyPromoting) {
			// register as sponsor for parent
			MarshalByRefObject mbo = parent as MarshalByRefObject;
			if (mbo != null) RegisterSponsor(mbo);
		}

		public override void Dispose() {
			MarshalByRefObject mbo = parentProvider as MarshalByRefObject;
			if (mbo != null) UnregisterSponsor(mbo);
			base.Dispose();
		}

		protected void RegisterSponsor(MarshalByRefObject instance) {
			if (instance == null) return;
			if (! RemotingServices.IsTransparentProxy(instance)) {
				ILease lease = RemotingServices.GetLifetimeService(instance) as ILease;
				if (lease != null)
					lease.Register(this);
			}
		}

		protected void UnregisterSponsor(MarshalByRefObject instance) {
			if (instance == null)
				return;
			if (!RemotingServices.IsTransparentProxy(instance)) {
				ILease lease = RemotingServices.GetLifetimeService(instance) as ILease;
				if (lease != null)
					lease.Unregister(this);
			}
		}

		public TimeSpan AdditionalLeaseTime {
			get { return leaseTime; }
			set { leaseTime = value; }
				
		}

		TimeSpan ISponsor.Renewal(ILease lease) {
			return leaseTime;
		}

		protected override void OnRegisterService(ServiceContainerEventArgs e) {
			base.OnRegisterService(e);
			try {
				MarshalByRefObject mbo = e.Instance as MarshalByRefObject;
				if (mbo != null) RegisterSponsor(mbo);
			} catch (System.Security.SecurityException ex) {
				// ������ ��� ������ ���������� ���� ������. ����������.
				System.Diagnostics.Trace.WriteLine(ex);
			}
		}

		protected override void OnUnregisterService(ServiceContainerEventArgs e) {
			try {
				MarshalByRefObject mbo = e.Instance as MarshalByRefObject;
				if (mbo != null) UnregisterSponsor(mbo);
			} catch (System.Security.SecurityException ex) {
				// ������ ��� ������ ���������� ���� ������. ����������.
				System.Diagnostics.Trace.WriteLine(ex);
			}
			base.OnUnregisterService(e);
		}
	}

	/// <summary>��������������� �����, ����������� ������������ ���������� <see cref="IServiceProvider"/> �
	/// <see cref="CallContext"/> ��� �������� ��� � ����� ����, ������� �� ��������� <see cref="IServiceProvider"/>
	/// ��� ��������.</summary>
	/// <remarks>������ �����, ������� ����� ���� �������������� ������-�� ���������, ����� ��������
	/// <see cref="IServiceProvider"/>. � ������, ���� �����, �� �����-�� ������� �� �������� ������ ���������,
	/// � ���������� �� ������ ����� ��������������� <see cref="IServiceProvider"/>, ����� ����������
	/// ������������������ <see cref="IServiceProvider"/>, ����������� ��� � <see cref="CallContext"/> � ���������� �-��, �
	/// ������� ��� � ���������.</remarks>
	/// <example>
	///		<code>
	///		IServiceProvider sp = new ServiceContainer();
	///		sp.AddService(typeof(IFoo), new Foo());
	/// 	sp.AddService(typeof(IBar), new Bar());
	///		using (new ProviderPublisher(sp)) {
	///			MethodWithoutSPParameter();
	///		}
	///
	///		void MethodWithoutSPParameter() {
	///			IServiceProvider sp = ProviderPublisher.Provider;
	///			IFoo foo = sp.GetService(typeof(IFoo));
	///			...
	///		}
	///		</code>
	/// </example>
	public class ProviderPublisher : IDisposable {
		const string key = "Front.ProviderPublisher:ServiceProvider";
		IServiceProvider	previous;
		AppDomain			domain;


		/// <summary>Inialize a new instance of the <see cref="ProviderPublisher"/> and
		/// publish <see cref="IServiceProvider"/> in <see cref="CallContext"/>.</summary>
		/// <param name="sp">The <see cref="IServiceProvider"/> for publishing to <see cref="CallContext"/>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="sp"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		public ProviderPublisher(IServiceProvider sp):this(sp, null) { }

		/// <summary>Inialize a new instance of the <see cref="ProviderPublisher"/> and
		/// publish <see cref="IServiceProvider"/> in specified <see cref="AppDomain"/>.</summary>
		/// <param name="sp">The <see cref="IServiceProvider"/> for publishing to <see cref="CallContext"/>.</param>
		/// <param name="ad">The <see cref="IServiceProvider"/> will be published to this <see cref="AppDomain"/> </param>
		/// <exception cref="ArgumentNullException"><paramref name="sp"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		public ProviderPublisher(IServiceProvider sp, AppDomain ad) {
			if (sp == null) throw new ArgumentNullException("sp");
			domain = ad;
			Publish(sp);
		}

		/// <summary>Unpublish current <see cref="IServiceProvider"/> from <see cref="CallContext"/>.</summary>
		public void Dispose() {
			Unpublish();
		}

		
		/// <summary>Publish <paramref name="sp"/> as current <see cref="IServiceProvider"/> to <see cref="CallContext"/>.</summary>
		/// <param name="sp">The <see cref="IServiceProvider"/> for publishing to <see cref="CallContext"/>.</param>
		protected virtual void Publish(IServiceProvider sp) {
			if (domain == null) {
				previous = CallContext.GetData(key) as IServiceProvider;
				CallContext.SetData(key, sp);
			} else {
				previous = domain.GetData(key) as IServiceProvider;
				domain.SetData(key, sp);
			}
		}

		/// <summary>Unpublish current <see cref="IServiceProvider"/> from <see cref="CallContext"/>.</summary>
		protected virtual void Unpublish() {
			if (domain == null) {
				if (previous == null)
					CallContext.FreeNamedDataSlot(key);
				else
					CallContext.SetData(key, previous);
			} else
				domain.SetData(key, previous);
		}

		/// <summary>Get current published <see cref="IServiceProvider"/>.</summary>
		/// <value>The current published <see cref="IServiceProvider"/>.</value>
		public static IServiceProvider Provider {
			get {
				IServiceProvider sp = CallContext.GetData(key) as IServiceProvider;
				if (sp == null) sp = AppDomain.CurrentDomain.GetData(key) as IServiceProvider;
				return sp;
			}
		}
	}

	/// <summary>����������, ����������� �������� �������� ��� �������������� ��������� ������� ��
	/// ������������� ����������.</summary>
	/// <see cref="ServiceNotFoundException"/>
	[Serializable]
	public class InvalidEnvironmentException : LocalizableException {
		/// <summary>�������������� ����� ��������� ���������� <see cref="InvalidEnvironmentException"/>.</summary>
		/// <remarks>���� ����������� �������������� <see cref="LocalizableException.ErrorCode"/> ��������� <b>ErrInvalidEnvironment</b></remarks>
		public InvalidEnvironmentException():this("ErrInvalidEnvironment") { }

		/// <summary>�������������� ����� ��������� ���������� <see cref="InvalidEnvironmentException"/>.</summary>
		/// <param name="errorCode">��� ������.</param>
		/// <param name="args">��������� ��� ����������� � �������������� ��������� �� ������.</param>
		/// <remarks> ���� <paramref name="errorCode"/> ����� null (Nothing � VB.NET) ��� ������ ������, �� ����� ������� �������� <b>ErrInvalidEnvironment</b>.</remarks>
		public InvalidEnvironmentException(string errorCode, params object[] args)
			:base( ((errorCode == null || errorCode.Length == 0) ? "ErrInvalidEnvironment" : errorCode), args) { }

		/// <summary>�������������� ����� ��������� ���������� <see cref="InvalidEnvironmentException"/>.</summary>
		/// <param name="inner">��������� ����������.</param>
		/// <param name="errorCode">��� ������.</param>
		/// <param name="args">��������� ��� ����������� � �������������� ��������� �� ������.</param>
		/// <remarks> ���� <paramref name="errorCode"/> ����� null (Nothing � VB.NET) ��� ������ ������, �� ����� ������� �������� <b>ErrInvalidEnvironment</b>.</remarks>
		public InvalidEnvironmentException(Exception inner, string errorCode, params object[] args)
			:base(inner, ((errorCode == null || errorCode.Length == 0) ? "ErrInvalidEnvironment" : errorCode), args) { }

		/// <summary>�������������� ����� ��������� ����������. ���������� ������� <see cref="ISerializable"/>.</summary>
		protected InvalidEnvironmentException(SerializationInfo info, StreamingContext context):base(info, context) { }
		
	}

	/// <summary>����������, ����������� �������� �������� ��� � �������������� ��������� ������� ��
	/// ������ ������, ����������� ��� ���������� ������.</summary>
	/// <see cref="InvalidEnvironmentException"/>
	[Serializable]
	public class ServiceNotFoundException : InvalidEnvironmentException {
		/// <summary>�������������� ����� ��������� ���������� <see cref="ServiceNotFoundException"/>.</summary>
		/// <remarks>���� ����������� �������������� <see cref="LocalizableException.ErrorCode"/> ��������� <b>ErrSomeServiceNotFound</b></remarks>
		public ServiceNotFoundException():this("ErrSomeServiceNotFound") { }

		/// <summary>�������������� ����� ��������� ���������� <see cref="ServiceNotFoundException"/>.</summary>
		/// <param name="serviceType">��� �������, ����� �������� ���������� ��������.</param>
		public ServiceNotFoundException(Type serviceType):this("ErrServiceNotFound", serviceType) { }

		/// <summary>�������������� ����� ��������� ���������� <see cref="ServiceNotFoundException"/>.</summary>
		/// <param name="errorCode">��� ������.</param>
		/// <param name="args">��������� ��� ����������� � �������������� ��������� �� ������.</param>
		/// <remarks> ���� <paramref name="errorCode"/> ����� null (Nothing � VB.NET) ��� ������ ������, �� ����� ������� �������� <b>ErrSomeServiceNotFound</b>.</remarks>
		public ServiceNotFoundException(string errorCode, params object[] args)
			:base(((errorCode == null || errorCode.Length == 0) ? "ErrSomeServiceNotFound" : errorCode), args) { }

		/// <summary>�������������� ����� ��������� ���������� <see cref="ServiceNotFoundException"/>.</summary>
		/// <param name="inner">��������� ����������.</param>
		/// <param name="errorCode">��� ������.</param>
		/// <param name="args">��������� ��� ����������� � �������������� ��������� �� ������.</param>
		/// <remarks> ���� <paramref name="errorCode"/> ����� null (Nothing � VB.NET) ��� ������ ������, �� ����� ������� �������� <b>ErrSomeServiceNotFound</b>.</remarks>
		public ServiceNotFoundException(Exception inner, string errorCode, params object[] args)
			:base(inner, ((errorCode == null || errorCode.Length == 0) ? "ErrSomeServiceNotFound" : errorCode), args) { }

		/// <summary>�������������� ����� ��������� ����������. ���������� ������� <see cref="ISerializable"/>.</summary>
		protected ServiceNotFoundException(SerializationInfo info, StreamingContext context):base(info, context) { }
		
	}
}
