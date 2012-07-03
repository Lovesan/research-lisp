using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Collections.Specialized;

namespace Front.ObjectModel {

	/// <summary>��������� ������� ������� � ���������� ������ (��� ����������� ������-������).</summary>
	public interface IDataContainer {
		// TODO:��� ������ �������� "Raw" ��������? ��� ������� �� � ����������������?
		object RawGetSlotValue(string slotName);
		object RawSetSlotValue(string slotName, object value);
	}


	/// <summary>������� ��������� ������</summary>
	public class DataContainer : HybridDictionary, IDataContainer {
		public DataContainer() {}

		public virtual object RawGetSlotValue(string slotName) {
			object result = null;
			if (slotName != null)
				result = Unwrap(slotName, this[slotName]);
			return result;
		}

		public virtual object RawSetSlotValue(string slotName, object value) {
			if (slotName == null)
				Error.Warning(new ArgumentNullException("slotName"), typeof(LObject));
			else {
				value = Wrap(slotName, value);
				this[slotName] = value; // ��������� ��������� ��������� ����� �����
			}
			return value;
		}


		#region Protected Methods
		//................................................................
		protected virtual object Wrap(string slotName, object value) {
			IObject obj = value as IObject;
			if (obj != null)
				return new RefferenceHandle(obj);
			return value;
		}

		protected virtual object Unwrap(string slotName, object value) {
			if (value == null || value == DBNull.Value)
				return null;
			// TODO: ��� Reference/Dereference ����� ����������� ������� "�������"

			// TODO: � ������ ���� ����� ���� ������ ValueHandle? ������� � �������?
			// ����� ������� ����� "��������������/�������������" ���������� ��� ���������� �������
			ValueHandle rf = value as ValueHandle;
			return (rf != null) ? rf.Value : value;
		}
		//................................................................
		#endregion
	}

}
