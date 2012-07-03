using System;
using System.Runtime.Serialization;
using Front.Collections;
using System.Collections.Generic;
using System.Collections;

namespace Front.ObjectModel {

	/// <summary>�������� ������</summary>
	// TODO: ����� ������������ �� List<SlotDefinition> ?
	// ������� ���� ����� NamedValueCollection....

	// TODO: ����� ������� ����� ������� Extension'�, ������� � ������������ �� ������� � ����� �������
	// TODO: ����� ������� ������ "�����" ������, ����� ��� ��������� ������� ������������,
	// ��� ���������� ������������

	public class ClassDefinition : SchemeNode, ISerializable {

		#region Protected Fields
		//.........................................................................
		protected bool CopyExtensions = true;
		protected string InnerName; // ��� � ������ Namespace?
		protected string InnerClassName; // ������ ���
		protected string InnerFullName; // version
		protected Guid InnerVersion = Guid.NewGuid();

		// ������ ������ ����������, � ������� ��������� � ��!
		// ���������: E3 -> E2 -> E1 -> �� -> S3 -> S2 -> S1		
		protected FixedOrderDictionary InnerFullInheritanceList = null;

		// ��������� ������ ���������������� ���������� � ����
		protected FixedOrderDictionary InnerInheritanceList = new FixedOrderDictionary();

		// TODO: ����� ���-�� ������� ������ ����������� ������!
		// ����� ����� ���������� (��� ��������� ������� ������������ �� �� ������ 
		// ������������ ���������� ������
		protected Hashtable InnerSlots = new Hashtable(); // -> SlotDefinition
		protected Hashtable InnerMethods = new Hashtable(); // -> MethodDefinition

		protected BehaviorDispatcher InnerBehavior;
		//.........................................................................
		#endregion


		#region Constructors
		//.........................................................................
		protected ClassDefinition() {}

		public ClassDefinition(SchemeNode parent, string name, bool copyExtensions, ClassDefinition[] extensions, params SlotDefinition[] slots) 
			: base(parent) {

			if (parent == null)
				InnerIsSchemeEditable = false;

			CopyExtensions = copyExtensions;

			InnerName = name;
			InnerFullName = Version.ToString("D");

			if (extensions != null && extensions.Length > 0)
				foreach (ClassDefinition ext in extensions)
					if (!InnerInheritanceList.Contains(ext.Name))
						InnerExtend(ext, false, true);
			
			InnerInheritanceList[KeyName] = this;
				// ���� � InheritanceList-� ����� FullName, �� ����� ������ ����������� 
				// ������ ��� ����������.
				// ���������, ���-���� �����, ��� �� ��������������� � ������� "������������" ������� �������
				// �� �� �������, ����� ��� �����!
			
			// XXX ��� Add, ������� �� [] (Pilya)
			//KeyName,
			//	this);

			if (slots != null && slots.Length > 0)
				foreach (SlotDefinition slot in slots)
					if (slot != null)
						InnerAddSlot(slot, true, true);

			if (CopyExtensions)
				InnerBehavior = new BehaviorDispatcher(this);
			else
				;// ?? ���� ������!
		}

		public ClassDefinition(SchemeNode parent, string name, ClassDefinition[] extensions, params SlotDefinition[] slots) 
			: this(parent, name, true, extensions, slots) { }

		public ClassDefinition(string name, bool copyExtensions, ClassDefinition[] extensions, params SlotDefinition[] slots)
			:this(null, name, copyExtensions, extensions, slots) { }

		public ClassDefinition(string name, bool copyExtensions, params ClassDefinition[] extensions)
			: this(null, name, copyExtensions, extensions, null) { }

		public ClassDefinition(string name, ClassDefinition[] extensions, params SlotDefinition[] slots) 
			: this(null, name, extensions, slots) { }

		public ClassDefinition(string name, ClassDefinition extension, params SlotDefinition[] slots) 
			: this(name, new ClassDefinition[] { extension }, slots) { }

		public ClassDefinition(string name, params SlotDefinition[] slots) 
			: this(null, name, (ClassDefinition[])null, slots) { }

		public virtual void GetObjectData(SerializationInfo info, StreamingContext context) {
			// TODO: ��������!
		}
		//.........................................................................
		#endregion


		// XXX �������� ���������� ����� ��������� ���������������� �� ��������� �����
		// (�� �������� ��������� ���-�� ���������� �������� ���������!)
		public event EventHandler<SlotListChangedEventArgs> AfterSlotListChanged;
		public event EventHandler<MethodListChangedEventArgs> AfterMethodListChanged;
		public event EventHandler<ExtensionListChangedEventArgs> AfterExtensionListChanged;


		#region Public Propreties
		//.........................................................................
		public string ClassName {
			// TODO ����� ������ �� ������ ����! �� � ��������� ��� ������ ���������! :-(
			// ��� �������, ����� ������������� "������� ���": ������� + ��� + ������
			get { return GetClassName(); }
		}

		public string Name {
			get { return GetName(); }
		}

		public string FullName {
			get { return GetFullName(); }
		}

		public Guid Version {
			get { return InnerVersion; }
		}

		public FixedOrderDictionary InheritanceList {
			// TODO: ��� ����� �������� ����� ������ ���� ReadOnly-�������!
			get {
				if (InnerFullInheritanceList == null)
					InnerFullInheritanceList = PopulateInheritanceList(new FixedOrderDictionary(), -1);
				return InnerFullInheritanceList;
			}
		}

		public List<SlotDefinition> Slots {
			get { return GetSlots(); }
		}

		public SlotDefinition this[string name] {
			get { return GetSlot(name); }
		}

		public BehaviorDispatcher Behavior {
			get { return InnerBehavior; }
		}

		public List<MethodDefinition> Methods {
			get { return GetMethods(); }
		}
		//.........................................................................
		#endregion


		#region Public Methods
		//.........................................................................
		public virtual void AttachBehavior(BehaviorDispatcher bd) {
			if (bd != null) {
				bd.Parent = InnerBehavior;
				InnerBehavior = bd;
			}
		}

		public virtual BehaviorDispatcher DetachBehavior() {
			if (InnerBehavior != null && InnerBehavior.Parent != null)
				InnerBehavior = InnerBehavior.Parent;

			return InnerBehavior;
		}

		public override int GetHashCode() {
			return InnerVersion.GetHashCode();
		}

		public override bool Equals(object obj) {
			if (obj == null)
				return false;
			ClassDefinition cd = obj as ClassDefinition;
			if (cd == null)
				return false;

			// TODO: �����������! ���������� � GetHashCode...
			// ����� ����� ��������� ������������ �� ��, ��� ������������ �������� � ��� �������� ���������������!
			// (���� ���� � ���, ��� �� ����� ��� ��������� �������� ���� �� �� ���)
			return (InnerVersion == cd.Version);
		}

		public virtual SlotDefinition AddSlot(SlotDefinition slot) {
			return InnerAddSlot(slot, true, true);
		}

		public virtual SlotDefinition RemoveSlot(string slotName) {
			return InnerRemoveSlot(slotName, true, true);
		}

		public virtual List<SlotDefinition> GetSlots() {
			List<SlotDefinition> res = new List<SlotDefinition>();

			Hashtable slts = InnerSlots;
			// XXX ���-�� �� ��������� ��������! 
			if (!CopyExtensions) {
				slts = new Hashtable();
				GetSlots(slts);
			}

			foreach (SlotDefinition sd in slts.Values)
				res.Add( sd );

			return res;
		}

		protected virtual Hashtable GetSlots(Hashtable slts) {
			//if (CopyExtensions) {
			//	foreach (DictionaryEntry de in InnerSlots)
			//		slts[de.Key] = de.Value;
			//
			//} else {
				foreach (ClassDefinition cls in InnerInheritanceList.Values) {
					if (cls == this) {
						foreach (DictionaryEntry de in InnerSlots)
							slts[de.Key] = de.Value;
					} else
						cls.GetSlots(slts);
				}
			//}
			return slts;
		}

		public virtual List<SlotDefinition> GetOwnSlots() {
			List<SlotDefinition> res = new List<SlotDefinition>();
			
			// ����� �� ���������� ������� ����� ��� ����������!
			string name = (CopyExtensions) ? Name : FullName;

			foreach (SlotDefinition sd in InnerSlots.Values)
				if (sd.DeclaredClass == name)
					res.Add(sd);
			return res;
		}

		public virtual SlotDefinition GetSlot(Name slotName) {
			return GetSlot(slotName);
			// TODO: ����� ������������ "����"
		}

		public virtual SlotDefinition GetSlot(string slotName) {
			// TODO: ��� �������� � �����������?

			SlotDefinition res = null;

			if (!CopyExtensions) {
				// TODO: ���������� ������: � Extension'��, � ����, � ���������
				// ������ ������ ����� ��������� � �������� �������,

				foreach(ClassDefinition cd in InnerInheritanceList.Values) {					
					res = (cd == this)
								? (SlotDefinition)InnerSlots[slotName]
								: cd.GetSlot(slotName);

					if (res != null) return res;
				}
			} else
				res = (SlotDefinition)InnerSlots[slotName];

			return res;
		}

		public virtual MethodDefinition AddMethod(MethodDefinition method) {
			return InnerAddMethod(method, true, true);
		}

		public virtual void AddMethods(IEnumerable<MethodDefinition> methods) {
			if (methods != null)
				foreach (MethodDefinition md in methods)
					AddMethod(md);
		}

		public virtual MethodDefinition RemoveMethod(string methodName) {
			return InnerRemoveMethod(methodName, true, true);
		}

		/// <summary>������ ����������� ������� ������. ���� ����� ������ - ��. BehaviorDispatcher</summary>
		public virtual List<MethodDefinition> GetMethods() {
			List<MethodDefinition> res = new List<MethodDefinition>();
			foreach (MethodDefinition m in InnerMethods.Values)
				res.Add(m);

			// TODO: ����� ������������, ��� �� �� �������� ������ ���...
			return res;
		}

		public virtual MethodDefinition GetMethod(string methodName) {
			MethodDefinition res = (MethodDefinition)InnerMethods[methodName];
			return res;
		}

		public virtual void Extend(params ClassDefinition[] extensions) {
			if (extensions != null && extensions.Length > 0)
				Extend(false, extensions);
		}

		public virtual void Extend(bool parent, params ClassDefinition[] extensions) {
			if (extensions == null || extensions.Length == 0) return;

			ArrayList a = new ArrayList();
			foreach (ClassDefinition ext in extensions) {
				if (InnerExtend(ext, false, parent))
					a.Add(ext);
			}
			if (a.Count > 0) {
				OnExtension(ListChangeType.Add, (ClassDefinition[])a.ToArray(typeof(ClassDefinition)));
			}
		}

		public virtual FixedOrderDictionary PopulateInheritanceList(FixedOrderDictionary ext, int index) {
			if (ext == null)
				ext = new FixedOrderDictionary();

			string name = KeyName;

			// XXX ��� ����������, ��� �� ������ ������� ����� � ������, � ������ ���� ������ ������������� � 0...
			if (index >= 0)
				ext.InsertAt(index, name, this);

			//for (int i = 0; i < InnerInheritanceList.Count; i++) {
			foreach (string cname in InnerInheritanceList.Keys) {
				ClassDefinition cd = (ClassDefinition)InnerInheritanceList[cname];
				if (cd != this)
					cd.PopulateInheritanceList(ext, (index < 0) ? index : ext.GetKeyIndex(name));
				else if (index < 0)
					ext[name] = this;
			}

			return ext;
		}

		public override string ToString() {
			return string.Format("{0} Class: {1}. {2} Slots. {3} Methods", GetType().Name, Name, Slots.Count, Methods.Count);
		}
		//.........................................................................
		#endregion


		#region Protected Methods
		//.........................................................................
		protected string KeyName {
			get { return (InnerName != null && InnerName != "") ? InnerName : InnerFullName; }
		}

		protected virtual string GetName() {
			// ���� ��� � ������ �� ������, �� �� ����� ��� ������� ��������
			// ���� ��������� ���� - �� ��� = ������

			if (InnerName != null && InnerName != "" ) return InnerName;
			if (!CopyExtensions && InnerInheritanceList.Count > 0) {
				ClassDefinition cd = InnerInheritanceList[0] as ClassDefinition;;
				if (cd != null && cd != this) {
					string cd_name = cd.Name;
					string cd_f_name = cd.FullName;
					if (cd_name != cd_f_name) return cd_name;
				}
			}
			return InnerFullName;
		}

		protected virtual string GetClassName() {
			if (InnerClassName != null && InnerClassName !="") return InnerClassName;
			if (InnerName != null) {
				int x = InnerName.IndexOf("-instance");
				if (x >=0)
					InnerClassName = InnerName.Substring(0, x);
				else
					InnerClassName = InnerName;
			}
			return InnerClassName;	
		}

		protected virtual string GetFullName() {
			string name = GetName();
			if (name != InnerFullName)
				return name + "-" + InnerFullName;
			return InnerFullName;
		}


		protected virtual bool InnerExtend(ClassDefinition extension, bool raiseEvent, bool parent) {
			if (extension == null || extension == this) return false;

			string name = KeyName;

			int ext_index = InnerInheritanceList.GetKeyIndex(extension.Name);
			int my_index = (parent) ? InnerInheritanceList.GetKeyIndex( name ) : -1;

			// ��� ��������� ������ �������� ��� "my_index", 
			// ����� ����� ����� ������ ������� ����� �������
			if (ext_index > 0 && my_index > 0 && my_index >= ext_index) 
				// ����� ��� ���� � ������ ������������ ����� ����
				return false;

			InnerFullInheritanceList = null; // ��� �� ����������� �����

			if (CopyExtensions) {
				// �������� ������ ������������ "�����" ��������� ��������
				// (���� my_index < 0, �� ������� � �����)

				//extension.PopulateInheritanceList( InnerInheritanceList, my_index);
				// { ��� �������� "������� ������ ������ ���������������� ���������"
				if (parent && my_index >= 0)
					InnerInheritanceList.InsertAt(my_index, extension.Name, extension);
				else 
					InnerInheritanceList[extension.Name] =  extension;
				// }

				if (ext_index < 0) {
					UpdateSlotListAfterExtention(ListChangeType.Add, extension);

				} else {
					// TODO: ���� extension �����������, �� ������ ����������� ������ ����
					// ������ �������������..
					// � �������...
				}

			} else {
				if (parent && my_index >= 0)
					InnerInheritanceList.InsertAt(my_index, extension.Name, extension);
				else
					InnerInheritanceList[extension.Name] = extension;
				// XXX ����� �� ����� ���������� �����������?
				// extension.AfterSlotListChanged += OnExtensionSlotListChanged;
			}

			if ( raiseEvent )
				OnExtension(ListChangeType.Add, extension);

			return true;
		}

		protected virtual SlotDefinition InnerAddSlot(SlotDefinition slot, bool riseEvent, bool replace) {
			if (CheckReadOnlyScheme())
				return null;

			if (slot != null && slot.Name != null && slot.Name != "") {
				bool f = false;
				lock (InnerSlots) {
					if (!InnerSlots.ContainsKey(slot.Name)) {
						InnerSlots.Add(slot.Name, slot);
						f = true;
					} else if (replace) {
						InnerSlots[slot.Name] = slot;
						f = true;
					}
				}
				if (f) { // ���������� ����� ������� ��� ����������� ������!
					if (slot.DeclaredClass == null) {
						// ��� ����� ��� ���������? ����� � DeclaredClass ������ ������ Version?
						slot.DeclaredClass = (CopyExtensions) ? Name : FullName;
						slot.ParentNode = this;
					}
					if (slot.ParentNode == null)
						slot.ParentNode = this; // XXX �� �� ����� �� ������ �����!

					if (riseEvent)
						OnSlotListChanged(ListChangeType.Add, slot);
				}
				return slot;
			}
			return null;
		}

		/// <summary>���������� ����� ����, ���� �������� ������� � "�������� ����� ������� ��������"</summary> 
		protected virtual SlotDefinition InnerRemoveSlot(string slotName, bool riseEvent, bool analize) {
			if (slotName == null || slotName.Trim() == "") return null;
			SlotDefinition slot = null;
			SlotDefinition res = null;
			lock (InnerSlots) {
				slot = InnerSlots[slotName] as SlotDefinition;
				if (slot != null)
					InnerSlots.Remove(slotName);
			}
			if (slot != null) {
				if (analize) 
					// TODO: ��������!
					;

				if (riseEvent) // �� �������� �������, ���� ������ ������������ ��������
					OnSlotListChanged(ListChangeType.Remove, slot);
			}
			return res;
		}

		protected virtual MethodDefinition InnerAddMethod(MethodDefinition method, bool riseEvent, bool replace) {
			if (method == null) return null;

			InnerMethods[method.Name] = method;
			if (method.DeclaredClass == null || method.DeclaredClass == "")
				method.DeclaredClass = (CopyExtensions) ? Name : InnerFullName;

			MethodDefinition res = InnerBehavior.AttachMethod(this, method);
			if (riseEvent)
				OnMethodListChanged(ListChangeType.Add, method);

			return res;
		}

		protected virtual MethodDefinition InnerRemoveMethod(string methodName, bool riseEvent, bool replace) {
			throw new NotImplementedException();
		}





		protected virtual void OnSlotListChanged(ListChangeType ct, params SlotDefinition[] slt) {
			EventHandler<SlotListChangedEventArgs> h = AfterSlotListChanged;
			if (h != null)
				h(this, new SlotListChangedEventArgs(ct, slt));
		}

		protected virtual void OnMethodListChanged(ListChangeType ct, params MethodDefinition[] methods) {
			EventHandler<MethodListChangedEventArgs> h = AfterMethodListChanged;
			if (h != null)
				h(this, new MethodListChangedEventArgs(ct, methods));
		}

		protected virtual void OnExtension(ListChangeType ct, params ClassDefinition[] extensions) {
			EventHandler<ExtensionListChangedEventArgs> h = AfterExtensionListChanged;
			if (h != null) {
				ExtensionListChangedEventArgs args = new ExtensionListChangedEventArgs(ct, extensions);
				h(this, args);
			}
		}

		/// <summary>TODO: ����� �������� � ����������� - ���� �� �������!</summary>
		protected virtual void OnExtensionSlotListChanged(object sender, SlotListChangedEventArgs args) {

			// TODO: ����� ������������ EventArgs'�� - ��� �����
			if (!CopyExtensions) {
				if (args.ChangeType == ListChangeType.Add || args.ChangeType == ListChangeType.Inherit)
					OnSlotListChanged(ListChangeType.Inherit, args.Slots);
				else
					OnSlotListChanged(args.ChangeType, args.Slots);

			} else if (args.ChangeType == ListChangeType.Add || args.ChangeType == ListChangeType.Inherit) {
				foreach (SlotDefinition s in args.Slots)
					// TODO: ����� ����� ����������� ������. ��� ����� ��������� ������������!
					// ���� ��������� ���� � �����, ������� � ������ ������������ ���� ����
					// ������, ��� ����� ������������� (���-�� ���-�� �����?)
					InnerAddSlot(s, false, true); 

				OnSlotListChanged(ListChangeType.Inherit, args.Slots);

			} else if (args.ChangeType == ListChangeType.Remove) {
				foreach (SlotDefinition s in args.Slots)
					// TODO: �������� ����� � ������ �������� ����� �������� "����������" ������� 
					// ����������� ����� �� ������� ��������.
					InnerRemoveSlot(s.Name, false, true);
			}
		}

		protected virtual void OnExtensionMethodListChanged(object sender, MethodListChangedEventArgs args) {
			// ��������������� Dispatcher
		}

		protected virtual void OnExtensionExtend(object sender, ExtensionListChangedEventArgs args) {
			// TODO: ����������� ������ ������ � �������...
			InnerFullInheritanceList = null;
			UpdateSlotListAfterExtention(args.ChangeType, args.Extensions);
			OnExtension(ListChangeType.Update, args.Extensions);
		}

		protected virtual void UpdateSlotListAfterExtention(ListChangeType ct, params ClassDefinition[] exts) {
			if (exts == null || exts.Length == 0) return;

			// �������� � ���� ��� ����� ���������
		foreach (ClassDefinition extension in exts) {
				if (ct == ListChangeType.Remove) {
					// TODO: �������� (��. ��� �� RemoveSlot
					;
				} else {
					foreach (SlotDefinition sd in extension.Slots) {
						// TODO: ��� ������ � �����������?

						// XXX �� ���������! ����� ���������, ��� ��������� extention 
						// � ������ ������������, �� ��� ����� cd.ClassDeclared...
						InnerSlots[sd.Name] = sd;
					}

					// ������� �������, ����� ���������� �����������, ��� ��
					// �� ���� �������� �����������...

					extension.AfterSlotListChanged -= OnExtensionSlotListChanged;
					extension.AfterMethodListChanged -= OnExtensionMethodListChanged;
					extension.AfterExtensionListChanged -= OnExtensionExtend;

					// TODO: ���������� ���������!
					extension.AfterSlotListChanged += OnExtensionSlotListChanged;
					extension.AfterMethodListChanged += OnExtensionMethodListChanged;
					extension.AfterExtensionListChanged += OnExtensionExtend;
				}
			}
		}
		//.........................................................................
		#endregion


		#region Supplementary Methods
		//.........................................................................
		//public static List<ClassDefinition> MakePrecedenceList(List<ClassDefinition> list, ClassDefinition definition) {
		//    if (list == null)
		//        list = new List<ClassDefinition>();

		//    if (definition != null) {
		//        list.Add(definition);
		//        foreach (ClassDefinition cd in definition.InheritanceList)
		//            MakePrecedenceList(list, cd);
		//    }

		//    return list;
		//}
		//.........................................................................
		#endregion

	}


	public enum ListChangeType {
		None,    // ������ �� ��������� - ���, ��������
		Add,     // ����������
		Remove,  // ���������
		Update,  // ���-�� ���������� 
		Inherit  // �������� ����� ������ (���� ����������)
	}


}
