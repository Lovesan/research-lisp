using System;
using System.Collections.Generic;
using System.Text;

namespace Front.Processing {

	// TODO: ��������� �������� ��������� ������������ �������!
	// TODO: ��������� EventQ/Event � Command/CommandActionEventHandler

	// ������� ���������� ������� "����� �� ��������� ������� ������"
	// (������, ������ - true, � false - ���� ����� ��������� ��� ����� ��������� ����)
	public delegate bool EventProcessor(Event e);


	[Serializable]
	/// <summary>
	/// ������, ������� ���������� � ������� ��������� ��� ���������� 
	/// � �������� ������� � ������������� �������.
	/// </summary>
	public class Event {
		// TODO: ID ����� ��������
		public long ID; // ��� �����?
		
		/// <summary>����� ������ [name.space].[���]
		/// ��������� ������� ����� ������������� ��� ����� namespace-�
		/// </summary>
		public string Name;

		public string SessionID; // TODO: ����� ���� �� ������ ����!
		public int Status; // XXX ��� ���? ������ int � �� enum?

		public object Sender;
		public object[] Args;


		protected Event() {}

		public Event(object sender, string name): this (sender, name, new object[]{}) {
		}

		public Event(object sender, string name, params object[] args) {
			Sender = sender;
			Name = name;
			Args = args;
		}
	}
}