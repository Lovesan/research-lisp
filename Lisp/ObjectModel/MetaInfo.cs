using System;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Text;

using Front.ObjectModel;



namespace Front.ObjectModel {

	public interface IMetaInfo {
		SchemeNode SchemeNode { get; }

		ClassDefinition GetClass(string cname);
		ClassDefinition GetClassVersion(string cname, Guid version);
		ClassDefinition RegisterClass(ClassDefinition cls);
		void RemoveClass(string className);

		IList<string> GetClassNames();

		IExtender GetExtender(string name);
		IExtender RegisterExtender(IExtender extender);
		bool RemoveExtender(string name);

		void Extend(string className, string extenderName, params object[] args);
	}

	/// <summary>��������� �������� �������. ������������ ����������� �������.</summary>
	// XXX: ��� ��� ������? ������ ��� ������? 
	// ��� ��� ������ ������� ��������, ���� ��� ����������� ������ ��� ���������?
	// (�� ������! ��� ���������� �������� � ������� ��� ��� "������� �����������")
	public class MetaInfo: InitializableBase, IMetaInfo {

		/* �������� ������ �������� �������, ����������, GF � �.�.
		 * � ��� �� �� ����������� (� ����� ����� �����������?)
		 * ������ ��������� ������ � "ReadOnly" ������
		 * (TODO: ��� �� ����� 2 ������ ������ "RO" � "Editable" �����
		 * ��� ����� RO-�������, ���� �������� �������� "RO/�� RO" ������ ���������
		 * �����-�� ��������... ��� �������� ������ �������)
		 */

		// ������ ������� ����������� ������. ��������� ������ - ���������� ������
		protected HybridDictionary InnerClasses = new HybridDictionary();
		protected SchemeNode InnerRootNode;
		protected HybridDictionary InnerExtenders = new HybridDictionary();

		public MetaInfo() {
			InnerRootNode = new SchemeNode();
		}

		// TODO: ��������!


		// MetaInfo �� ����������� �� SchemeNode, �� ������������� �����
		// (������������ - ��������� ����.. �� ���� "������������")
		public SchemeNode SchemeNode {
			get { return InnerRootNode; }
		}


		#region IMetaInfo Methods
		//.........................................................................
		public virtual ClassDefinition GetClass(string cname) {
			return InnerGetClass(cname, Guid.Empty);
		}

		public virtual ClassDefinition GetClassVersion(string cname, Guid version) {
			return InnerGetClass(cname, version);
		}

		// TODO RegisterClass ����� ��� ������ ����������� ������ read-only. ����� ����� �� ������ MetaInfoConfigurator
		public virtual ClassDefinition RegisterClass(ClassDefinition cls) {
			if (cls != null)
				InnerSetClass(cls.Name, cls);
			return cls;
		}

		public virtual void RemoveClass(string className) {
			InnerSetClass(className, null);
		}

		public virtual IList<string> GetClassNames() {
			IList<string> res = new List<string>();
			foreach (DictionaryEntry e in InnerClasses) {
				if (e.Value != null)
					res.Add((string)e.Key);
			}
			return res;
		}

		public virtual IExtender GetExtender(string name) {
			return InnerGetExtender(name);
		}

		public virtual IExtender RegisterExtender(IExtender ext) {
			if (ext != null)
				InnerRegisterExtender(ext.Name, ext);
			return ext;			
		}

		public virtual bool RemoveExtender(string name) {
			if (name == null) return false;
			name = name.Trim().ToLower();
			if (name == "") return false;
			if (!InnerExtenders.Contains(name)) return false;

			InnerExtenders.Remove(name);

			return true;
		}

		public virtual void Extend(string className, string extenderName, params object[] args) {
			IExtender ext = GetExtender(extenderName);
			if (ext != null)
				ext.Apply(GetClass(className), args);
		}
		//.........................................................................
		#endregion

		
		#region Protected Methods
		//.........................................................................
		protected virtual ClassDefinition InnerGetClass(string name, Guid version) {
			// TODO: ����� �������, ��� �� ������ �� Class.FullName ���� ��������� �����������!
			if (name == null) return null;
			name = name.Trim().ToLower();
			if (name == "") return null;

			ArrayList cl = InnerClasses[name] as ArrayList;
			if (cl == null || cl.Count == 0) return null;
			if (version == Guid.Empty)
				return cl[0] as ClassDefinition;
			else {
				foreach (ClassDefinition cd in cl) {
					if (cd != null && cd.Version.Equals(version))
						return cd;
				}
			}
			return null;
		}

		// TODO: ������� ThreadSafe!
		protected virtual string InnerSetClass(string name, ClassDefinition cd) {
			if (cd == null) return null;
			if (name == null) name = cd.Name;

			if (name == null) return null; // TODO: � ���!
			name = name.Trim().ToLower();
			if (name == "")	return null;

			ArrayList cl = InnerClasses[name] as ArrayList;
			if (cl == null)
				InnerClasses[name] = cl = new ArrayList();

			cl.Insert(0, cd);
			return name;
		}

		protected IExtender InnerGetExtender(string name) {
			if (name == null) return null;
			name = name.Trim().ToLower();
			if (name == "") return null;

			IExtender extender = InnerExtenders[name] as IExtender;
			return extender;
		}

		protected virtual void InnerRegisterExtender(string name, IExtender ext) {
			if (ext == null) return;
			if (name == null) name = ext.Name;
			if (name == null) return;

			name = name.Trim().ToLower();
			if (name == "") return;

			InnerExtenders[name] = ext;
		}
		//.........................................................................
		#endregion

		public static IMetaInfo Current {
			get {
				IServiceProvider sp = ProviderPublisher.Provider;
				if (sp != null)
					return sp.GetService(typeof(IMetaInfo)) as IMetaInfo;
				return null;
				// TODO: � ������ ������ "ServiceNotFoundException"
			}
		}

	}

}
