using System;
using System.Collections.Generic;
using System.Text;
using Front.Lisp.Debug;


namespace Front.Lisp {

	/// <summary>��������� ����������� ������ ���������� ����������� �������������� � Lisp-�������.
	/// LispLoader ������ ������� LispCmdExec �� Debugger � ������ add-in</summary>
	public interface ILispIterop {
		NodeDescriptor Eval(string str);
		NodeDescriptor EvalQ(string str);

		string SymbolValue(string name);
		/// <summary>������ ���������� ������</summary>
		string[] Files();
		/// <summary>����������� �����</summary>
		string File(string name);

		/// <summary>��������� ����</summary>
		void LoadFile(string path);

		/// <summary>���������� � ������</summary>
		bool CheckAvailability { get; }

		/// <summary>��������� ������ � ����</summary>
		void Intern(string symname, object symbol);

		/// <summary>�������� ���� �� �����</summary>
		NodeDescriptor GetNodeByKey(long key);

		/// <summary>�������� ������� ���� �� �����</summary>
		ArrayListSerialized GetAllChilds(long key);

		ArrayListSerialized TraceArc(long key, string arkName);
		
		// XXX: ��� �� ������ ��������� � ���� ��������� ���������� ��
		// �������� StackFrame?

		// TODO: �������� ������ ��������� ������ � �����!

		// TODO: ����������� ������ ��� ��������� �������/��������� �������� ������
		// � ��� ����� � ������������/��������������...
		// (!) ��� ������ � DTE ����� ������������ ������-�����, ������� ����� �������� � ���������
		// ������ � ����� ������������� ��, ��� ������ DTE
	}
	
}
