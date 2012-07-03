// $Id: Initialization.cs 1628 2006-07-14 14:04:42Z pilya $

using System;
using System.Collections;
using System.Collections.Specialized;

namespace Front {

	/// <summary>��������� ��� ������� � ������� ��������������.</summary>
	/// <remarks>��������� ������, �������� ������ ����������������� �������, �� ����� ����
	/// ������������������� � ������������ � ������� ������ ���� �������������. ��� ��������������
	/// ������� ����� ������ ������������ <see cref="IInitializable"/>.</remarks>
	public interface IInitializable {
		/// <summary>��������� ��� ������ ��������� ���������������.</summary>
		/// <value>true, ���� ������ ��������������� � ����� � ������������ � false, ���� ��� ���������
		/// ����� <see cref="Initialize"/>.</value>
		bool IsInitialized { get; }
		/// <summary>���������. ��������� �� ������ � �������� ������������� �������������.</summary>
		/// <value>true, ���� ������ ��������� � �������� ������������� �������������.</value>
		bool IsInitializing { get; }
		/// <summary>������������������� ������.</summary>
		/// <param name="sp"><see cref="IServiceProvider"/>, ������� ������������� ������� ��� ������� �������.</param>
		/// <remarks>������ ������ ��������� ��������� ����� <see cref="Initialize"/>. � ���� ������ �� �����
		/// ��������� ����������������� ��� ������ ��������������� �����.
		/// <para>���� ����� <see cref="IServiceProvider"/> ��� ������������� ������� ��������� ����� ����������,
		/// ����������� ����� <see cref="ParametersBag"/>.</para>
		/// </remarks>
		/// <example><code>
		/// void ForceObjectInitialization(IInitializable obj) {
		///		ParametersBag sp = new ParametersBag(myServiceProvider);
		///		sp.Parameters["src"] = @"C:\Temp";
		///		sp.Parameters["dst"] = @"D:\Work";
		///		obj.Initialize(sp);
		/// }
		/// </code></example>
		void Initialize();
		void Initialize( IServiceProvider sp );

		// TODO DF0004: ����������� �������� �� ��������������!
		// TODO DF0009: ������� Before ���������� CancelHandler'��
		event EventHandler BeforeInitialize;
		event EventHandler AfterInitialize;

	}



	/// <summary>���������� <see cref="IServiceProvider"/> � <see cref="System.ComponentModel.Design.IServiceContainer"/>, �������
	/// �������� ��� � ��������� ����������� ����������. ������������ ��� ������������� �� ���������� <see cref="IInitializable"/>.</summary>
	public class ParametersBag : ServiceContainer {
		IDictionary parameters;
		
		/// <summary>������� ����� ��������� <see cref="ParametersBag"/>.</summary>
		public ParametersBag():this(null) { }
		
		/// <summary>������� ����� ��������� <see cref="ParametersBag"/> � �������� ���
		/// ������������ <see cref="System.ComponentModel.Design.IServiceContainer"/>.</summary>
		public ParametersBag(IServiceProvider parent) {
			//
			parameters = new ListDictionary();
		}

		/// <summary>������ ����������.</summary>
		public IDictionary Parameters { get { return parameters; } }
	}

	

	/// <summary>��������� ��� ���������� ����������� ������� ������������� �� ���������� IInitializable.</summary>
	public class InitializeEventArgs : EventArgs {
		public IServiceProvider SP;
		public InitializeEventArgs(IServiceProvider sp) {
			this.SP = sp;
		}
	}

	

	// TODO DF0005: ��������
	public class InitializationException : LocalizableException {
		public InitializationException(string m) : base(m) {}
	}



	/// <summary>������� ����� ��� ���������������� ��������� ��������.</summary>
	/// <remarks>(����� �������� ����������� <see cref="MarshalByRefObject"/>.)</remarks>
	public abstract class InitializableBase : MarshalByRefObject, IInitializable, IDisposable {
		protected IServiceProvider InnerSP;
		public IServiceProvider SP { get { return InnerSP; } }

		// TODO DF0006: ��������� ������� ������ �������������.
		protected InitializableBase() { }
		protected InitializableBase(IServiceProvider sp): this(sp, false) {
		}
		protected InitializableBase(IServiceProvider sp, bool init) {
			if (init) 
				Initialize(sp);
			else
				this.InnerSP = sp;
		}

        int init_lock = 0;
		public virtual void Initialize() {
			Initialize(ProviderPublisher.Provider);
		}

		public virtual void Initialize(IServiceProvider sp) {
			// TODO DF0007: ������� ������ �� ������������ ������������� � ���������� ������ 
			//    � ������������� ��������� (done. ���������!)
			// TODO ��0008: ������� ������� �� ���������� ��������� "Silent" (��������� ������ ��������)
			//		���� ������ ���� ������ �������� - �� ������ �� � ������ ������ � ���� ��������.
			//		��������� �������� ������� � ������ ������.
			try {
				int x = System.Threading.Interlocked.Increment(ref init_lock);
				if (x > 1)
					throw new InitializationException("Concurrent initialization");

				if (IsInitialized)
					throw new InitializationException("Repeated initialization");

				// ���� ����� ��� ������ � ServiceProvider, �� � ��������� ��������������...
				if (SP != null && sp == null) sp = SP;
				EventHandler bi = BeforeInitialize;
				if (bi != null) bi(this, new InitializeEventArgs(sp));

				bool res = OnInitialize(sp);
				if (res) {
					EventHandler ai = AfterInitialize;
					if (ai != null) ai(this, new InitializeEventArgs(sp));
				}
				InnerIsInitialized = res;
			} 
			//catch (Exception ex) {
			//    //System.Windows.Forms.MessageBox.Show(ex.ToString());
			//    int i = 0;
			//}
			finally {
                System.Threading.Interlocked.Decrement(ref init_lock);
                init_lock = 0;
			}
		}

		public virtual void Dispose() { }

		// �������� �����, ������� ����� �������� � �����������
		protected virtual bool OnInitialize(IServiceProvider sp) {
			this.InnerSP = sp;
			return true;
		}

		public override object InitializeLifetimeService() {
			return null;
		}

		protected bool InnerIsInitialized = false;
		public virtual bool IsInitialized { get { return InnerIsInitialized; } }

		public virtual bool IsInitializing { get { return (init_lock > 0); } }

		public event EventHandler BeforeInitialize;
		public event EventHandler AfterInitialize;
	}
}
