using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Runtime.Serialization;

namespace Front.ObjectModel {

	public interface ICollectionObject : IObject, IList {
		string ItemClass { get; }
	}

	// TODO: ������� ������ ������������� � ������ ���������� IExtendable!
	// ������, ��� �� DataCollectionContaier ����� �������� ������ IObject (��������� ����� � ��
	// �����, � ����� ���� �� ����� ���������!)

	// TODO: ������� � �������������� IExtendable ����� ���� ���������� ���. �������� (���� ������� �����!)

	/// <summary>������ ������ - ��������� ��������</summary>
	
	[Serializable]
	public class CollectionLObject<T> : LObjectBase, ISerializable, ICollectionObject, IList<T>, ICollectionContainer {

		protected bool InnerIsReadOnly = false;
		protected ClassDefinition InnerItemClass;

		#region Constructors
		//.............................................................
		public CollectionLObject() : base() { }

		public CollectionLObject(ClassDefinition definition) : base(definition) { }

		public CollectionLObject(ICollectionContainer container) : this(null, container) { }

		public CollectionLObject(ClassDefinition definition, ICollectionContainer container) : base(definition, container) {
		}

		protected CollectionLObject(SerializationInfo info, StreamingContext context) : base(info, context) {
			string className = info.GetString("InnerItemClass");
			// TODO: �������� CustomExtentions, ������ �� ����� ����� ���� ���������� � Update, ��� �� ��������� ���������� ���������!
			InnerIsReadOnly = info.GetBoolean("InnerIsReadOnly");
		}

		public virtual void GetObjectData(SerializationInfo info, StreamingContext context) {
			base.GetObjectData(info, context);
			// TODO: ��������!
			//InnerClass ������� ������ ClassName
			if (InnerItemClass == null)
				info.AddValue("InnerItemClass", "");
			else
				info.AddValue("InnerItemClass", InnerItemClass.Name);

			// TODO: ����� �������� - ��� "�������" ��� ������� ����-����������
			//    1. ������ �� ����� �������� �������� ���� "class1-instance", � ��� ���������� ����������� �������� ������ ��� ������
			//    2. ����� �������� ����� ����� ������������� ����������, ������� ��� � Metailnfo-������, � �� ������ ����� �������������
			//       �������������!
			// info.AddValue("CustomExtentions", GetCustomExtentions(InnerClass));

			info.AddValue("InnerIsReadOnly", InnerIsReadOnly);
		}
		//.............................................................
		#endregion


		#region ICollectionObject Members
		//.............................................................
		public virtual string ItemClass {
			get { return GetItemClass(); }
		}
		//.............................................................
		#endregion


		#region ICollectionContainer Members
		//.............................................................
		public virtual object RawGetValue(int index) {
			return InnerList.RawGetValue(index);
		}

		public virtual object RawSetValue(int index, object value) {
			return InnerList.RawSetValue(index, value);
		}

		public virtual CollectionEntry GetEntry(int index) {
			return InnerList.GetEntry(index);
		}

		public virtual CollectionEntry GetEntry(object key) {
			return InnerList.GetEntry(key);
		}

		public virtual CollectionEntry NewEntry(params object[] args) {
			return InnerList.NewEntry(args);
		}
		//.............................................................
		#endregion


		#region IList Members
		//.............................................................
		int IList.Add(object value) {
			return InnerList.Add( Wrap(value, false) );
		}

		bool IList.Contains(object value) {
			// ���� ��� �� �������� - ������ ���������� false
			object v = Wrap(value, true);
			if (v != null)
				return Contains((T)v);
			return false;
		}

		int IList.IndexOf(object value) {
			return IndexOf(Wrap(value, false));
		}

		void IList.Insert(int index, object value) {
			Insert(index, Wrap(value, false));
		}

		void IList.Remove(object value) {
			object v = Wrap(value, true);
			if (v != null)
				Remove((T)v);
		}

		object IList.this[int index] {
			get { return this[index]; }
			set { RawSetValue(index, Wrap(value, false)); }
		}

		public virtual void Clear() {			
			InnerList.Clear(); // XXX ��-��-��!
		}

		public virtual bool IsFixedSize {
			get {
				throw new Exception("The method or operation is not implemented.");
			}
		}

		public virtual bool IsReadOnly {
			get { return InnerIsReadOnly; }
		}

		public virtual void RemoveAt(int index) {
			InnerList.RemoveAt(index);
		}
		//.............................................................
		#endregion


		#region ICollection Members
		//.............................................................
		public virtual void CopyTo(Array array, int index) {
			throw new Exception("The method or operation is not implemented.");
		}

		public virtual int Count {
			get { return InnerList.Count; }
		}

		public virtual bool IsSynchronized {
			get { return InnerList.IsSynchronized; }
		}

		public virtual object SyncRoot {
			get { return InnerList.SyncRoot; }
		}
		//.............................................................
		#endregion


		#region IList<T> Members
		//.............................................................
		public virtual int IndexOf(T item) {
			return InnerList.IndexOf(item);
		}

		public virtual void Insert(int index, T item) {
			throw new Exception("The method or operation is not implemented.");
		}

		public virtual T this[int index] {
			get { return Wrap( RawGetValue(index), true ); }
			set { RawSetValue(index, UnWrap(value)); }
		}
		//.............................................................
		#endregion


		#region ICollection<T> Members
		//.............................................................
		public virtual void Add(T item) {
			InnerList.Add(item);
			//throw new Exception("The method or operation is not implemented.");
		}

		public virtual bool Contains(T item) {
			throw new Exception("The method or operation is not implemented.");
		}

		public virtual void CopyTo(T[] array, int arrayIndex) {
			throw new Exception("The method or operation is not implemented.");
		}

		public virtual bool Remove(T item) {
			InnerList.Remove(item);
			return true; // XXX �������� ���!
			//throw new Exception("The method or operation is not implemented.");
		}
		//.............................................................
		#endregion


		#region IEnumerable, IEnumerable<T> Members
		//.............................................................
		public virtual IEnumerator<T> GetEnumerator() {
			// XXX �� ����� ������� �������.
			// �� 2 CollectionContainerEnumerator<T> ���� ������ �������
			// ...����, ��� ������� ������ �� ����� ������������ ���� � ������
			// (���� ������� ����� ���� �����, � ����� � ���� ���� ����������... � ����� ���� �� �����������)
			return new CollectionContainerEnumerator<T>(this, InnerList.GetEnumerator());
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return InnerList.GetEnumerator();
		}
		//.............................................................
		#endregion


		#region Protected Methods
		//.............................................................
		protected virtual ICollectionContainer InnerList {
			get { return InnerDataContainer as ICollectionContainer; }
		}

		protected override IDataContainer InitializeDataContainer() {
			InnerDataContainer = new CollectionContainer();
			return InnerDataContainer;
		}

		protected virtual string GetItemClass() {
			throw new NotImplementedException();
		}

		/// <summary>���������� ������ � ������� ���� (� ������ ���������� IExtendable!)</summary>
		protected virtual T Wrap(object value, bool quite) {

			// TODO: ����� ���������� Wrap ���, ��� �� IObjectWrapper ����������� � IObject
			// ����� ��� ���������� � ��������� Behavior'�� (������� IObject) ������
			// ��������� ���, ������ �� Nut'��
			// (���� ����� �������� ��� ������ �� ����� DataContainer'� � RefferenceHandle)

			if (value == null) return default(T);
			if (value is T) return (T)value;

			IExtendable ex = value as IExtendable;
			if (ex != null) {
				object o = ex.As<T>();
				if (o != null)
					return (T)o;
			}
			if (quite)
				return default(T);
			throw new InvalidCastException();
		}

		protected virtual object UnWrap(T value) {
			return value;
		}
		//.............................................................
		#endregion

	}


	public class CollectionLObject : CollectionLObject<object> {

		#region Constructors
		//.............................................................
		public CollectionLObject() : base() {
		}

		public CollectionLObject(ClassDefinition definition) : base(definition) {
		}

		public CollectionLObject(ICollectionContainer container) : this(null, container) {
		}

		public CollectionLObject(ClassDefinition definition, ICollectionContainer container)
			: base(definition, container) {
		}
		//.............................................................
		#endregion

	}

	// TODO: ����� ����������� Enumerator

}
