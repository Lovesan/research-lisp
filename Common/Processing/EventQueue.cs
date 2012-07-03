using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.ComponentModel;

namespace Front.Processing {

	/// <summary>
	/// ������� �������. �������� ��������� ������� ���������� � �������������� �������.
	/// ��� ������� ������ ������� - ��� ������������� � ������� � ���������� ���������������
	/// ������ � ��������� ������ (������������ ��� �������).
	/// ������� ��������� �������� ��������� ����������� ������� � ����������� 
	/// ������ (ISynchronizeInvoke),������� ��� ������ ��� ����������� �����������.
	/// </summary>
	public class EventQueue : ProcessingQueue<Event> {
		// TODO: ����� ������� ������� �������������� � ���� ICommand-�

		//protected IDictionary<int, DispatchNode> InnerDispatchTable = new Dictionary<int, DispatchNode>();

		protected EventDispatchNode RootNode = new EventDispatchNode();

		// ������ ������������������� ������ ��� ����������������� ������������!
		// (������-�� ��� ����� "������� ����", ��� �� �������� ������ � �������
		//  ���������� � ������� ������)
		public ISynchronizeInvoke Sync; 

		public EventQueue() : base(10000) {
			Start();
		}


		#region Public Methods
		//............................................................................
		public virtual void RegisterHandler(string code, EventProcessor handler) {
			RootNode.SetHandler(code, handler);
		}

		public virtual void RegisterHandler(string code, EventProcessor handler, ISynchronizeInvoke sync) {
			RootNode.SetHandler(code, handler, sync);
		}

		public virtual void RegisterSyncHandler(string code, EventProcessor handler) {
			if (code == null || code =="" || handler == null) return;
			if (Sync != null)
				RootNode.SetHandler(code, handler, Sync);
			else
				RootNode.SetHandler(code, handler);
		}

		public virtual void RemoveHandler(string code, EventProcessor handler) {
			RootNode.RemoveHandler(code, handler);
		}

		public virtual void RemoveHandlers(string code) {
		}

		public virtual void Raise(string code, params object[] args) {
			Enqueue(new Event(null, code, args));
		}

		public virtual void Raise(object sender, string code, params object[] args) {
			Enqueue(new Event(sender, code, args));
		}

		public virtual void Raise(Event e) {
			Enqueue(e);
		}

		public virtual void RaiseSync(string code, params object[] args) {
			HandleEvent(new Event(null, code, args));
		}

		public virtual void RaiseSync(object sender, string code, params object[] args) {
			HandleEvent(new Event(sender, code, args));
		}

		public virtual void RaiseSync(Event e) {
			HandleEvent(e);
		}
		//............................................................................
		#endregion



		protected override int TryProcess() {
			Event item = Current;
			if( item == null ) return 0;
			
			HandleEvent(item);
			return 0;
		}

		protected virtual void HandleEvent(Event e) {
			// TODO: �������� ��������� ����� ��� ���������!
			RootNode.Invoke(e);
		}


		public static EventQueue CurrentQueue {
			get {
				IServiceProvider sp = ProviderPublisher.Provider;
				if (sp != null)
					return (EventQueue)sp.GetService(typeof(EventQueue));
				return null;
			}
		}


		/// <summary>���� � ������� ��������� ������� �� ������� ������. � ��� �������� ���������� � ������� �������������� ��� "����������� ������������"</summary>
		public class EventDispatchNode {
			protected Hashtable InnerNodes = new Hashtable();

			// ��� ����������������� ������������ �� ������� ������������ ��������
			// ������� ����� ��� �������, ��� �� ����� ����� ����������� ����������
			protected Hashtable DynamicHandlers = new Hashtable();

			public EventDispatchNode() {
			}

			public EventDispatchNode(EventProcessor handler) {
				if (handler != null)
					Handler += handler;
			}


			public event EventProcessor Handler;


			public EventDispatchNode this[string name] {
				get {
					if (name == null || name == "")
						return null;
					return (EventDispatchNode)InnerNodes[name];
				}
			}

			// TODO: �� threadSafe!
			public virtual EventDispatchNode SetHandler(string name, EventProcessor handler) {
				return SetHandler(name, handler, null);
			}

			public virtual EventDispatchNode SetHandler(string name, EventProcessor handler, ISynchronizeInvoke sync) {
				if (name == null || name == "" || handler == null)
					return null;

				string[] parts = name.Split('.');
				EventDispatchNode node = this;

				foreach (string p in parts) {
					EventDispatchNode node1 = node[p];
					if (node1 == null) {
						node1 = new EventDispatchNode();
						node.InnerNodes[p] = node1;
					}
					node = node1;
				}
				
				if (sync != null) {
					EventProcessor h = DynamicHandlers[handler] as EventProcessor;
					if (h == null) {
						// ������� ��������� ����������, ������� ������� � ���������,
						// ����� handler �������� ���� � ���������� ������ ������������ �����
						EventProcessor h1 = handler;
						ISynchronizeInvoke sync1 = sync;

						h = delegate(Event ev) { 
								if (sync1 == null || h1 == null) return true; // ������ �������� - �����-�� �����
								object res0 = sync1.Invoke(h1, new object[] { ev });
								if (res0 != null && res0 is Boolean)
									return (bool)res0;
								else 
									return true;
							};
						DynamicHandlers[handler] = h;
					}
					handler = h;
				}
				if (handler != null) {
					node.Handler -= handler;
					node.Handler += handler;
				}

				return node;
			}


			public virtual void RemoveHandler(string name, EventProcessor handler) {
				if (name == null || name == "")
					return;

				string[] parts = name.Split('.');
				EventDispatchNode node = this;

				foreach (string p in parts) {
					node = node[p];
					if (node == null) return;
				}

				if (handler != null)
					node.Handler -= handler;
			}


			public virtual EventDispatchNode GetNode(string name) {
				if (name == null || name == "") return null;
				string[] parts = name.Split('.');

				EventDispatchNode node = this;
				foreach (string p in parts) {
					node = node[p];
					if (node == null) return null;
				}
				return node;
			}

			public virtual EventDispatchNode GetFirstNode(string name) {
				return GetNextNode(null, name);
			}

			public virtual EventDispatchNode GetNextNode(EventDispatchNode f, string name) {
				if (name == null || name == "") return null;
				string[] parts = name.Split('.');

				EventDispatchNode node = this;
				
				foreach (string p in parts) {
					node = node[p];
					if (node == null)
						return null;

					if (f != null) { // ������� ��������� �� ���� �����, ��� ����
						if (node == f)
							f = null;
						continue;
					}
					
					if (node.Handler != null)
						return node;
				}
				return null;
			}

			public virtual void Invoke(Event e) {
				//try {
					if (e == null) return;

					EventDispatchNode node = GetFirstNode(e.Name);
					bool c = true;
					while (c) {
						if (node != null) {
							EventProcessor h = node.Handler;
							if (h != null)
								c = h.Invoke(e);
							if (c)
								node = GetNextNode(node, e.Name);
						} else
							c = false;

					}
				//} catch (Exception ex) {
				//	Front.Diagnostics.Log.Default.Fail(ex);
				//	throw;
				//}
			}
		}


	}

}
