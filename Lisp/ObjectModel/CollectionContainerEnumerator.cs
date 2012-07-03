using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace Front.ObjectModel {

	// TODO: ����� ��� ���-�� ��������!
	// ���� ��� ����� ������������ �������� � DataSetContainer ��� ������ UnWrap!
	// ������ ���� ����� ������ ���� �����-�� ������� � ObjectScope.

	// (!) ��������: Enumerator ���� � ��� DataContainer'� � ��� ���������!
	// �� ��� ����� ���� �� ��������� (�������� ������������ ������ � InnnerCurrent)

	// TODO: ����� �� ThreadSafe!

	/// <summary>Enuerator ���������-����������. � �������� IEnumerator'� ����� ��� �� 
	/// ���������� �� �������� ���������, ������� �� ������������� ���� Generic'�!</summary>
	public class CollectionContainerEnumerator<T> : IEnumerator<T> {
		protected ICollectionContainer InnerCC;
		protected int InnerCurrent;
		protected IEnumerator InnerEnumerator;
		protected int BasePosition;


		#region Constructors
		//..............................................................
		protected CollectionContainerEnumerator() {
		}

		public CollectionContainerEnumerator( ICollectionContainer cc, int position) {
			InnerCC = cc;
			InnerCurrent = position;
			BasePosition = position;
		}

		public CollectionContainerEnumerator( ICollectionContainer cc) : this(cc, 0) {
		}

		public CollectionContainerEnumerator( ICollectionContainer cc, IEnumerator enumer) {
			InnerCC = cc;
			InnerEnumerator = enumer;
		}
		//..............................................................
		#endregion

		#region IEnumerator<T> Members
		//..............................................................
		public T Current {
			get { return UnWrapCurrent(); }
		}

		#endregion

		#region IDisposable Members
		//..............................................................
		public virtual void Dispose() {
		}
		//..............................................................
		#endregion


		#region IEnumerator Members
		//..............................................................
		object System.Collections.IEnumerator.Current {
			get { return GetCurrent(); }
		}

		public virtual bool MoveNext() {
			if (InnerEnumerator == null) {
				if (InnerCurrent < InnerCC.Count)
					InnerCurrent++;

				return (InnerCurrent < InnerCC.Count);
			} else
				return InnerEnumerator.MoveNext();
		}

		public virtual void Reset() {
			if (InnerEnumerator == null)
				InnerCurrent = BasePosition;
			else
				InnerEnumerator.Reset();
		}
		//..............................................................
		#endregion


		protected IObjectScope Scope {
			get { 
				IObjectScopeBound b = InnerCC as IObjectScopeBound;
				if (b != null)
					return b.ObjectScope;
				return null;
			}
		}

		protected virtual object GetCurrent() {
			object o = (InnerEnumerator != null)
							? InnerEnumerator.Current
							: InnerCC[InnerCurrent];

			IObjectScope scp = Scope;
			if (o != null && scp != null)
				o = scp.UnWrap(o);
			return o;
		}

		protected virtual T UnWrapCurrent() {
			object o = GetCurrent();

			if (o is T) {
				return (T)o;
			} else {
				IExtendable ex = o as IExtendable;
				if (ex != null)
					return ex.As<T>();
			}
			return default(T);
		}
	}

}
