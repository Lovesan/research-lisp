// $Id: Wrapper.cs 944 2006-06-08 15:14:49Z john $

using System;

namespace Front {

	/// <summary>����� ��������� ��� �������-�������.</summary>
	/// <remarks>���������� <see cref="IWrapper"/> ����� ��������� ��� ��������� ������ � ������������������ �������.</remarks>
	/// <seealso cref="Wrapper"/>
	public interface IWrapper {
		/// <summary>������������� ����������������� ������.</summary>
		/// <value>����� ���������� Null ���� ������ � ������� �������� ��� ����������.</value>
		object Wrapped { get; }

		/// <summary>�������� ����������������� ������ ���������� ����.</summary>
		/// <param name="type">��� �������� ������� (��� ����������, ������� ������ ������ ������������).</param>
		/// <returns>���� �� ����������������� ��������, ������� ������������� ��������� ���� ��� ��������� ���������.</returns>
		object GetWrapped(Type type);

		/// <summary>�������� ��������� ����������������� ������ ���������� ����.</summary>
		/// <param name="type">��� �������� ������� (��� ����������, ������� ������ ������ ������������).</param>
		/// <param name="obj">���������� ������</param>
		/// <returns>���� �� ����������������� ��������, ������� ������������� ��������� ���� ��� ��������� ���������.
		/// ������������, ���� ��������������� ��������� ��������. ���� <see cref="obj"/> �� ������ (����� �������� null),
		/// �� ������ ������������ ������ <see cref="GetWrapped">GetWrapped(Type type)</see>.</returns>
		object GetWrapped(Type type, object obj);
	}


	/// <summary>������ ���������� �������������� �������.</summary>
	/// <remarks>���� ��������� ��������� <see cref="IWrapper"/> ��������� � ���� �������� ���� ���������������� �������.</remarks>
	public interface IWrapper<T> : IWrapper {
		new T Wrapped { get; }
	}

	
	/// <summary>���������� �����-�������. ����� ������� ������� ��� ������ �������.</summary>
	/// <remarks>��������� ����������� ���������������� ��� ������ � ������ ���������.</remarks>
	[Serializable]
	public abstract class Wrapper : IWrapper {
		
		protected object InnerWrapped;

		protected Wrapper() { }

		/// <summary>������� ����� ������-�������.</summary>
		/// <param name="o">��������������� ������.</param>
		public Wrapper(object o) {
			InnerWrapped = o; 
		}

		public virtual object Wrapped { get { return InnerWrapped; } }

		/// <summary>���������� ��������� ������, ��� ������ ���������� ����.</summary>
		/// <param name="type">���, ������� ������ ����� ��������� ������.</param>
		/// <returns>��� ������-������� ��� ��������� ������, ���� ������� �� ������������
		/// ��������� ���.</returns>
		/// <remarks>��� ���������� ������ �������� ����������� ����� <see cref="Lookup"/>.</remarks>
		public virtual object GetWrapped(Type type) {
			return Wrapper.Lookup(this, type);
		}

		public virtual object GetWrapped( Type type, object obj) {
				// TODO DF0027: ��������! �������� ���� � �����������������!
				// ���� �������� ��� ������ �������� ������ ��������� ����� � ������
				// ���� ������� ��� ��� ����������� �����
				throw new NotImplementedException("TODO DF0027");
		}

		/// <summary>����� ���������� ����� ��������� �������� ����, ��� ����� ��������� ���.</summary>
		public static object Lookup(object w, Type t) {
			object res = null;
			while (res == null && w != null) {
				res = t.IsInstanceOfType( w ) ? w : null;
				if (res == null)
					w = (w is IWrapper) ? ((IWrapper)w).Wrapped : null;
			}
			return res;
		}
	}

	
	// TODO DF0001: ��������
	// �������, ���������!

	// ������� �������������� Wrapper
	[Serializable]
	public class Wrapper<T> : Wrapper, IWrapper<T> {
		protected Wrapper() {}

		protected Wrapper(object o) : base(o) { }
		public Wrapper(T o):base(o) { }
		
		new public virtual T Wrapped { get { return (T)base.Wrapped; } }
		object IWrapper.Wrapped { get { return base.Wrapped; } }
	}

	
	// TODO DF0002: ��������
	// ��� ��� InitializableBase ����������� �� MarshalByRefObject, �� 
	// ������������ ��� ��������� ��������, ����������� �� ����������
	public class ServiceWrapper<T> : InitializableBase, IWrapper<T>  {
		protected T InnerWrapped;
		protected bool wrappedSet = false;

		public ServiceWrapper() : base() {}
		
		public ServiceWrapper(T obj) : base() {
			InnerWrapped = obj;
			wrappedSet = true;
			AttachToWrapped();
		}

		public ServiceWrapper(IServiceProvider sp) : base(sp) { }

		public ServiceWrapper(IServiceProvider sp, bool init) : base(sp, init) { }

		protected override bool OnInitialize(IServiceProvider sp) {
			if (!wrappedSet)
				try {
					if (sp != null)
						InnerWrapped = (T)sp.GetService(typeof(T));
					if (InnerWrapped != null) {
						wrappedSet = true;
						AttachToWrapped();
					}
				} catch (Exception ex) { } 
			return base.OnInitialize(sp);
		}

		protected virtual void AttachToWrapped() {
		}

		protected virtual void DetachFromWrapped() {
		}

		public virtual T Wrapped { get { return InnerWrapped; } }
		object IWrapper.Wrapped { get { return this.Wrapped; } }

		public virtual object GetWrapped(Type type) {
			return Wrapper.Lookup(this, type);
		}

		public virtual object GetWrapped(Type type, object obj) {
			throw new NotImplementedException();
		}
	}
	
}
