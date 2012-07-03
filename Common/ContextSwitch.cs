// $Id: ContextSwitch.cs 864 2006-06-02 15:25:46Z kostya $

using System;
using System.Collections;
using System.Data;
using System.Text;
using System.Runtime.Remoting.Messaging;

namespace Front {

	/// <summary>������ ������ ������������ ���������.</summary>
	/// <remarks><para>��� ���������� �� ������������� �������� ���������� ����������, ������� �����
	/// ��������������� ��� <see cref="CallContext"/> ��� <see cref="AppDomain"/>.</para>
	/// <para>������������� ��������� ��������� <see cref="IDisposable"/> � ��������������� ����������
	/// �������� ��������� ��� �����������.</para>
	/// <example>
	/// 	...
	/// 	using (new ContextSwitch&lt;IDictionary&gt;(MyDictionary, "GlobalDict") ) {
	///			..
	///			ContextSwitch&lt;IDictionary&gt;.Current("GlobalDict");
	///			..
	///		}
	/// </example>
	/// </remarks>
	// TODO DF-67: ������� �� ���! � �������� �������� ������
	public class ContextSwitch< T > : IDisposable, INamedValue<T> {
		static string CommonKey = "Front.ContextSwitch:" + typeof( T ).Name;
		static bool _none_created = false;

		protected object	  previous;		// ContextSwitch< T >
		protected AppDomain domain;
		protected T         InnerValue;
		protected Name      InnerName = null;
		protected string    InnerKey = CommonKey;

		#region INamedValue<T> Implementation
		public string Name { get { return InnerName;}  set {} }
		public Name FullName { get { return InnerName; } }
		
		public T Value { get { return InnerValue; } set { InnerValue = value; } }
		#endregion
		
		public string Key { get { return InnerKey; } }

		protected ContextSwitch() { }

		public ContextSwitch( T newValue) : this(newValue, null, null) { }
		public ContextSwitch( T newValue, string name) : this(newValue, name, null) { }
		public ContextSwitch( T newValue, AppDomain ad) : this( newValue, null, ad) { }
		public ContextSwitch( T newValue, string name, AppDomain ad) {
			//if (newValue == null && name != "none" ) throw new ArgumentNullException("newValue");
			InnerName = new Name(name);

			InnerValue = newValue;
			InnerKey = (Name != null) ? (CommonKey + "[" + Name + "]") : CommonKey;
			domain = ad;
			Publish();
		}

		public virtual void Dispose() {
			Unpublish();
		}

		protected virtual void Publish() {
			if (domain == null) {
				previous = CallContext.GetData(Key);
				CallContext.SetData(Key, this);
			} else {
				previous = domain.GetData(Key);
				domain.SetData(Key, this);
			}
		}

		protected virtual void Unpublish() {
			if (domain == null) {
				if (previous == null)
					CallContext.FreeNamedDataSlot(Key);
				else
					CallContext.SetData(Key, previous);
			} else
				domain.SetData(Key, previous);
		}

		/// <summary>���������� ������� �������� ���������</summary>
		/// <remarks>�������� ����������� ������� � <see cref="CallContext"/>, � ����� � <see cref="AppDomain"/>.</remarks>
		public static T Current {
			get {
				return GetCurrent(null);
			}
		}

		/// <summary>���������� ������� ������������� ���������</summary>
		/// <remarks>�������� ����������� ������� � <see cref="CallContext"/>, � ����� � <see cref="AppDomain"/>.</remarks>
		public static ContextSwitch<T> CurrentSwitch {
			get { return GetCurrentSwitch(null); }
		}

		// TODO: ��� ����� ������� indexer'��!
		/// <summary>���������� ������� �������� ���������</summary>
		/// <remarks>�������� ����������� ������� � <see cref="CallContext"/>, � ����� � <see cref="AppDomain"/>.</remarks>
		public static T GetCurrent(string name) {
			ContextSwitch<T> c = GetCurrentSwitch(name);
			
			return (c != null )
				? c.Value : default(T);
		}

		/// <summary>���������� ������� ������������� ���������</summary>
		/// <remarks>�������� ����������� ������� � <see cref="CallContext"/>, � ����� � <see cref="AppDomain"/>.</remarks>
		public static ContextSwitch<T> GetCurrentSwitch(string name) {
			string key =  (name != null) ? (CommonKey + "[" + name + "]") : CommonKey;
			ContextSwitch<T> res = CallContext.GetData(key) as ContextSwitch<T>;
			if ( res == null) res = AppDomain.CurrentDomain.GetData(key) as ContextSwitch<T>;
			return res;
		}
	}

	

	public class ContextSwitch : ContextSwitch<object> {
		public ContextSwitch( object newValue) : base(newValue, null, null) { }
		public ContextSwitch( object newValue, string name) : base(newValue, name, null) { }
		public ContextSwitch( object newValue, AppDomain ad) : base( newValue, null, ad) { }
		public ContextSwitch( object newValue, string name, AppDomain ad) : base( newValue, name, ad) { }
	}
}


