using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace Front.ObjectModel {

	/// <summary>��������� ������� � ���������� ���������� ������ (������ ����� ���� � �������������)</summary>
	public interface ICollectionContainer : IDataContainer, IList {
		object RawGetValue(int index);
		object RawSetValue(int index, object value);

		CollectionEntry GetEntry(int index);
		CollectionEntry GetEntry(object key);

		/// <summary>����� ��� �������� ���������� ��� ������ �������� (�������� ��������� Unbound � ������ ���� ��������� � ���������)</summary>
		CollectionEntry NewEntry(params object[] args);
	}
	

	public class CollectionContainer : CollectionBase, ICollectionContainer {

		#region Constructors
		//.................................................................
		public CollectionContainer() {
		}

		// TODO: ������� ������������� � ��������� �����
		// ��� ���� ����� ��������� ������������� �����
		public CollectionContainer(params object[] args ) : this(args as IEnumerable) {
		}

		public CollectionContainer(IEnumerable data) {
			if (data != null)
				foreach (object o in data)
					InnerList.Add(o);
		}
		//.................................................................
		#endregion


		#region IList Members
		//.................................................................
		public virtual object this[int index] {
			get { return RawGetValue(index); }
			set { RawSetValue(index, value); }
		}

		public virtual bool IsFixedSize {
			get { return false; }
		}

		public virtual bool IsReadOnly {
			get { return false; }
		}

		public virtual int Add(object value) {
			lock (InnerList) {
				value = Wrap(this.Count, value);
				InnerList.Add(value);
				return this.Count;
			}
		}

		public virtual void Insert(int index, object value) {
			throw new NotImplementedException();
		}

		public virtual void Remove(object value) {
			throw new NotImplementedException();
		}

		public virtual bool Contains(object value) {
			return (IndexOf(value) >= 0);
		}

		public virtual int IndexOf(object value) {
			// TODO: ����� ������� ����� �����������:
			//   * �� ����� (��� ID-�),
			//   * �� ������ �������,
			//   * �� ������ �������
			throw new NotImplementedException();
		}

		//.................................................................
		#endregion


		#region ICollectionContainer Members
		//.................................................................
		/// <summary>���������, �������� �� <c>slotName</c> ������ � ���� �� - �� ��������� ���������� ��������������� ��������. 
		/// � �������� ����� ��� �� ������������ ������������������ ������ �� ���������� ������.</summary>
		public virtual object RawGetSlotValue(string slotName) {
			int index = -1;
			if (Int32.TryParse(slotName, out index) && index >=0 && index < this.Count)
				return RawGetValue(index);
			return null;
		}

		/// <summary>���������, �������� �� <c>slotName</c> ������ � ���� �� - �� ��������� ������������� ��������������� ��������. </summary>
		public virtual object RawSetSlotValue(string slotName, object value) {
			int index = -2;
			if (Int32.TryParse(slotName, out index)) {
				return RawSetValue(index, value);
			}
			return null;

		}

		/// <summary>���������� ��������, ���� <c>index</c> �� ������� �� ������� �������, ����� - null</summary>
		public virtual object RawGetValue(int index) {
			if (index >= 0 && index < this.Count)
				return Unwrap(index, InnerList[index]);
			return null;
		}

		/// <summary>������������� ��������, ���� <c>index</c> �� ������� �� ������� �������, ����� - ������������.
		/// �������� ������� -1 �������������� ��� ������� � ������. �������� �������, ������ Count, �������������� ��� ���������� � �����.</summary>
		public virtual object RawSetValue(int index, object value) {
			if (index >= 0 && index < this.Count) {
				value = Wrap(index, value);
				InnerList[index] = value;

			} else if (index == -1) {
				value = Wrap(index, value);
				InnerList.Insert(0, value);

			} else if (index == this.Count) {
				value = Wrap(index, value);
				InnerList.Add(value);

			} else
				return null;
			return value;
		}

		public virtual CollectionEntry GetEntry(int index) {
			// ����� ����, ��� ��������� Entry, ��� �������� �������� � ���������
			// (Unwrap ��� ��������� ��������� � ������ ������ ������)
			throw new NotImplementedException();
		}

		public virtual CollectionEntry GetEntry(object key) {
			throw new NotImplementedException();
		}

		public virtual CollectionEntry NewEntry(params object[] args) {
			throw new NotImplementedException();
		}
		//.................................................................
		#endregion


		#region IEnumerable Methods
		//................................................................
		public new virtual IEnumerator GetEnumerator() {
			return new CollectionContainerEnumerator<object>(this, InnerList.GetEnumerator());
		}
		//................................................................
		#endregion


		#region Protected Methods
		//................................................................
		protected virtual object Wrap(int index, object value) {
			IObject obj = value as IObject;
			if (obj != null)
				return new RefferenceHandle(obj);
			//����������� ����� ��� �� CollectionEntry!
			return value;
		}

		protected virtual object Unwrap(int index, object value) {
			if (value == null || value == DBNull.Value)
				return null;
			// TODO: ��� Reference/Dereference ����� ����������� ������� "�������"
			ValueHandle rf = value as ValueHandle;
			return (rf != null) ? rf.Value : value;
		}
		//................................................................
		#endregion
	}

}
