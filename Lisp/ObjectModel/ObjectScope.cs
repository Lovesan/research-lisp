using System;
using System.Collections.Generic;
using System.Text;

namespace Front.ObjectModel {


	public interface IObjectScope {
		IObjectScopeBound Get(object key);
		IObject Get(string className, long objectID);
		//T Get<T>(long objectID); ������� �����!

		IObjectScopeBound Merge(IObjectScopeBound obj, ScopeMergeMode mode);		
		// TODO: ����� ������ ��������� ������ �������� � ������ (�����, � �� �������������)
		// ��� �� ����� ������ ���� Merge, Attache � �.�.
		object UnWrap(object obj);

		IDataContainer NewDataContainer(ClassDefinition cls);
		IDataContainer NewDataContainer(string className);

		T New<T>();
	}


	public interface IObjectScopeBound {
		IObjectScope ObjectScope {
			get;
		}

		bool Attach(IObjectScope scope);
	}

	public enum ScopeMergeMode {
		// TODO: ���������� � ������������. �������� ��������!
		Add,
		Copy,
		Check,
		Diff,
		Strong,
		Replace
	}

	public delegate IDataContainer DataContailerFactoryDelegate(string clsname, object o, params object[] args);
}
