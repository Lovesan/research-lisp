using System;
using Front.Collections;
using System.Collections.Generic;

namespace Front.ObjectModel {
	
	///<summary>������� ����� ��� ��������� ��������������, ������������ �������� "���������������"</summary>
	// TODO: �������� ������� ���� �� ������ ��������������� �����.
	public class SchemeNode {

		protected bool InnerIsSchemeEditable = true;
		protected SchemeNode InnerParentNode = null;
		protected SchemeNode InnerRootNode = null;


		#region Constructors
		//................................................................................
		public SchemeNode() : this( false ) {}

		public SchemeNode(SchemeNode parentNode) {
			InnerParentNode = parentNode;
			InnerRootNode = (InnerParentNode != null) ? InnerParentNode.RootNode : null;
		}

		public SchemeNode(bool isEditable) {
			InnerIsSchemeEditable = isEditable;
		}
		//................................................................................
		#endregion


		// TODO: ���-�� ����� ��������� "������������", ���� ����� ����� ������ ���������
		// ��������, ���� ��� ��������, ���������� � ������� ����� �������� �� �����...
		// (������ ��� ������! � ����� �������� ����������... ��������, ���
		//  ������� ���������� ��������� ����� ������ � ��������� �������!)

		// ��� ��� �������� ������ ������, ��� ���� �� ����� ���� "���-����"...
		// ���� � ������ ���������� ������ - ���� �����!
		public event EventHandler AfterParentChanged;
		public event EventHandler AfterRootChanged;


		#region Public Properties
		//...............................................................................
		public SchemeNode ParentNode {
			get { return InnerParentNode; }
			set { InnerSetParentNode(value); }
		}

		public SchemeNode RootNode {
			get { return InnerRootNode; }
		}

		public bool IsSchemeEditable { 
			get { return !CheckReadOnlyScheme(); }
		}
		//...............................................................................
		#endregion


		// TODO: ���� ����� �������� � ���, ��� �� ���-�� ��������������� �� ���������� � ��������������
		//		��� ������ �������������� ���� ������? ��������� ����� ��� ���������� ����?
		//		��� ����� �������� �� ��������� �������.
		//		(����� Parent/Root ��� ����������� ��� ���������... �� ������� ���������� ����� Rocket Science)
		public virtual void SetSchemeEditable(bool editable) {
			InnerIsSchemeEditable = editable;
		}


		#region Protected Methods
		//...............................................................................
		protected virtual SchemeNode InnerSetParentNode(SchemeNode node) {
			lock (this) {
				if (!CheckReadOnlyScheme()) {
					DetachParent();

					InnerParentNode = node;
					SchemeNode sn = InnerRootNode;
					InnerRootNode = (InnerParentNode != null && InnerRootNode != InnerParentNode.RootNode)
								? InnerParentNode.RootNode : null;

					AttachParent(node);

					OnAfterParentChanged();

					if (sn != InnerRootNode) // �� ������ ����� ������ ����, ���� ������ ����������� �����!
						OnAfterRootChanged();
				}
			}
			return InnerParentNode;
		}

		protected virtual bool CheckReadOnlyScheme() {
			// TODO: ��������!
			if (InnerParentNode != null)
				return InnerParentNode.IsSchemeEditable;
			return InnerIsSchemeEditable;
		}

		/// <summary>������� ����������� ������� � ������������ ����</summary>
		protected virtual void DetachParent() {
			SchemeNode n = InnerParentNode;
			if (n == null) return;
			n.AfterParentChanged -= ParentChangedEventHandler;
			n.AfterRootChanged -= RootChangedEventHandler;
		}

		/// <summary>������������� ����������� ������� �� ������������ ����</summary>
		protected virtual void AttachParent(SchemeNode pnode) {
			if (pnode == null) return;
			pnode.AfterParentChanged += ParentChangedEventHandler;
			pnode.AfterRootChanged += RootChangedEventHandler;
		}

		protected virtual void OnAfterParentChanged() {
			EventHandler h = AfterParentChanged;
			if (h != null)
				h(this, EventArgs.Empty);
		}

		protected virtual void OnAfterRootChanged() {
			EventHandler h = AfterRootChanged;
			if (h != null)
				h(this, EventArgs.Empty);
		}

		protected virtual void ParentChangedEventHandler(object sender, EventArgs args) {
		}

		protected virtual void RootChangedEventHandler(object sender, EventArgs args) {
			// XXX ��� ����������� � InnerSetParentNode � �������� � ������ OnParentChanged...

			SchemeNode sn = InnerRootNode;
			InnerRootNode = (InnerParentNode != null && InnerRootNode != InnerParentNode.RootNode)
						? InnerParentNode.RootNode : null;

			if (sn != InnerRootNode) // �� ������ ����� ������ ����, ���� ������ ����������� �����!
				OnAfterRootChanged();
		}
		//...............................................................................
		#endregion
	}

}
