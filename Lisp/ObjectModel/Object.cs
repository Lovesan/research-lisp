// $Id$
// (c) Pilikn Programmers Group

using System;
using System.Collections;
using System.Data;
using System.Text;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.ComponentModel;

namespace Front.ObjectModel {

	public interface IObject : IDataContainer, ICloneable {
		ClassDefinition Definition { get; }

		/// <summary>���������� ���� �� �����. ���� ����� ��� - ���������� null. ����� ������������ � �������� Contains(slotname)</summary>
		SlotDefinition GetSlot(string slotName);

		/// <summary>���������� ������ ������ (������ ������ ����� �� ��������������� ������ ������ ������!)</summary>
		List<SlotDefinition> GetSlots();

		object GetSlotValue(string slotName);
		object SetSlotValue(string slotName, object value);

		event EventHandler<SlotChangeEventArgs> AfterSetSlotValue;
		event EventHandler<SlotChangeEventArgs> BeforeSetSlotValue;

		/// <summary>������� ���������� ���� �������� ������ ��� ��������� �������� ����� � ��� ������, ���� fixup �� �������� ������!</summary>
		event EventHandler<SlotErrorEventArgs> AfterSlotError;

		new IObject Clone();
	}

	public interface IExtendable {
		T As<T>();
		object As(System.Type t);
		object As(string cname); // ��� Extension'� � ���������� ������

		void Extend(object extension);
		void Shrink(System.Type t);

		// TODO: ��������� �������� ��������� ������ ���������� (��� ��������� � ��������������)
	}


	/// <summary>��������� ������ (������ ������), ������ �������� ������</summary>
	public class LObject : LObjectBase {

		protected LObject() : base() {}

		protected LObject( bool initialize ) : base( initialize ) {
		}

		public LObject(ClassDefinition definition) : base(definition) { }

		protected override IDataContainer InitializeDataContainer() {
			// TODO: ��� ����� �����!
			InnerDataContainer = new DataContainer();
			return InnerDataContainer;
		}
	}


	
}
