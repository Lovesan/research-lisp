// $Id$

using System;
using System.Collections;
using System.Data;
using System.Text;
using System.Runtime.Serialization;
using System.Collections.Generic;

namespace Front.ObjectModel {

	
	/// <summary>������� ����� ��� �������� ������. ������ �������� ������� � �����, �� �� ��������� ����� �������� ������</summary> 
	public abstract class LObjectBase : ISerializable, IBehavioredObject {

		/* �������� �������� ��������: 
		 *    1. ������, � ��������� � ClassDefinition, 
		 *    2. Hashtable
		 *    3. ������ + Hashtable
		 *    4. DataRow + DataSet ?
		 * 
		 * 	TODO: ��������, ��� ����� �������� null, DBNull, Empty, Uninitialized � Default ��������
		 */

		#region Protected Properties
		//................................................................
		protected ClassDefinition InnerClass;				
		protected BehaviorDispatcher InnerBehavior;			
		protected IDataContainer InnerDataContainer;		
		//................................................................
		#endregion


		#region Constructors
		//................................................................
		protected LObjectBase() : this( true ) {
		}

		protected LObjectBase( bool initialize ) {
			if (initialize)				
				InitializeDataContainer();
		}

		protected LObjectBase( ClassDefinition cls , IDataContainer container) : this(false) {
			// TODO: ����� ������ ��� � ��������� ClassWrapper?

			UpdateMetaInfo(cls);

			if (container != null)
				InnerDataContainer = container;
			else
				InitializeDataContainer();
		}

		protected LObjectBase( ClassDefinition cls ) : this( cls, true) {
		}

		protected LObjectBase( ClassDefinition cls, bool init ) : this(cls, null) { 
			// XXX � �������������� ����!!!
		}

		protected LObjectBase(SerializationInfo info, StreamingContext context)  {
			string className = info.GetString("InnerClassName");
			UpdateMetaInfo( MetaInfo.Current.GetClass(className) );
			// TODO: �������� CustomExtentions, ������ �� ����� ����� ���� ���������� � Update, ��� �� ��������� ���������� ���������!
			InnerDataContainer = info.GetValue("DataContainer", typeof(IDataContainer)) as IDataContainer;
		}

		public virtual void GetObjectData(SerializationInfo info, StreamingContext context) {
			// TODO: ��������!
			//InnerClass ������� ������ ClassName
			info.AddValue("InnerClassName", InnerClass.Name); 

			// TODO: ����� �������� - ��� "�������" ��� ������� ����-����������
			//    1. ������ �� ����� �������� �������� ���� "class1-instance", � ��� ���������� ����������� �������� ������ ��� ������
			//    2. ����� �������� ����� ����� ������������� ����������, ������� ��� � Metailnfo-������, � �� ������ ����� �������������
			//       �������������!
			// info.AddValue("CustomExtentions", GetCustomExtentions(InnerClass));
			
			info.AddValue("DataContainer", InnerDataContainer);
		}
		//................................................................
		#endregion


		public event EventHandler<SlotChangeEventArgs> AfterSetSlotValue;
		public event EventHandler<SlotChangeEventArgs> BeforeSetSlotValue;
		public event EventHandler<SlotErrorEventArgs> AfterSlotError;


		#region Public Properties
		//................................................................
		public ClassDefinition Definition { 
			get { return InnerClass; }
		}

		public virtual SchemeNode SchemeNode { 
			get { return InnerClass; }
		}

		public object this[string slotName] {
			get { return GetSlotValue(slotName); }
			set { SetSlotValue(slotName, value); }
		}
		//................................................................
		#endregion


		#region IObject Methods
		//................................................................
		public virtual SlotDefinition GetSlot(string slotName) {
			if (InnerClass != null)
				return InnerClass.GetSlot(slotName);
			return null;
		}

		public virtual List<SlotDefinition> GetSlots() {
			if (InnerClass != null)
				return InnerClass.GetSlots();
			return null;
		}

		public virtual object GetSlotValue(string pname) {
			SlotReaderDelegate r = GetReader(pname);
			if (r == null)
				return RawGetSlotValue(pname);
			else
				return r(this, pname);
		}

		public virtual object SetSlotValue(string pname, object value) {
			object original = null;
			try {
				original = RawGetSlotValue(pname); 
			} catch (Exception ex) {
				// TODO: ��� �� ������! ������ ���� �����-�� TryRawGetSlotValue!
			}

			if (OnBeforeSetSlotValue(pname, original, value)) {
				SlotWriterDelegate w = GetWriter(pname);
				try {
					Object nv = (w != null) 
						? w(this, pname, value) 
						: RawSetSlotValue(pname, value);
					OnAfterSetSlotValue(pname, original, nv);
					return nv;
				} catch (Exception ex) { // XXX  ����� ����� ��� ������ ����������!!!
					OnSlotError(pname, original, value, ex);
				}
			}
			return null;
		}
		//................................................................
		#endregion


		#region IDataContainer methods
		//................................................................
		public virtual object RawGetSlotValue(string slotName) {
			return InnerDataContainer.RawGetSlotValue(slotName);
		}

		public virtual object RawSetSlotValue(string slotName, object value) {
			return InnerDataContainer.RawSetSlotValue(slotName, value);
		}
		//................................................................
		#endregion


		#region IObject Members
		//.........................................................................		
		public virtual IObject Clone() {
			// TODO: ��������!
			throw new NotImplementedException();
		}

		object ICloneable.Clone() {
			return Clone();
		}
		//.........................................................................
		#endregion


		#region IBehaviored implementation
		//................................................................
		public virtual bool HasBehavior(string name) {
			return (GetBehavior(name) != null);
		}

		public virtual ObjectBehavior GetBehavior(string name) {
			// TODO: �������� � ����, ����� �������� � ������...
			// �� ������ ������������� �����-�� Mixer:
			// ������ ��� � ����, � ��������� ������. � ��������� ������� � � ������...
			// �������� BehaviorDispatcher ��� ��� ������� � ����, ����� ������ ����� ������ � ����!
			return InnerBehavior.GetBehavior(name) as ObjectBehavior;
		}

		public virtual MethodDefinition[] GetMethod(string name, params object[] args) {
			// TODO: �������� (��. GetBehavior)
			throw new NotImplementedException();
		}

		public virtual MethodDefinition[] GetMethod(string name, params Type[] args) {
			// TODO: �������� (��. GetBehavior)
			throw new NotImplementedException();
		}

		public virtual object Invoke(string method, params object[] args) {
			return InnerBehavior.Invoke(method, args);
		}

		public virtual bool CanInvoke(string method) {
			return InnerBehavior.CanInvoke(method);
		}

		public virtual ClassDefinition UpdateMetaInfo(ClassDefinition cls) {
			if (cls != null) {
				InnerClass = new ClassDefinition(cls, cls.Name + "-instance", false, new ClassDefinition[] { cls });
			} else {
				InnerClass = new ClassDefinition("noname");
			}
			InnerClass.AttachBehavior(new BehaviorDispatcher(cls, cls != null ? cls.Behavior : null, this));
			InnerBehavior = InnerClass.Behavior;
			return InnerClass;
		}
		//................................................................
		#endregion


		#region Protected Methods
		//................................................................
		protected abstract IDataContainer InitializeDataContainer();

		protected virtual SlotReaderDelegate GetReader(string pname) {
			SlotReaderDelegate d = null;
			if (pname != null) {
				string mname = string.Format("get_{0}", pname);
				MethodDefinition md = InnerBehavior.GetMethod(mname);
				if (md != null)
					d = delegate(object sender, string propName) {
						return InnerBehavior.Invoke(mname);
					};
			}

			return d;
		}

		protected virtual SlotWriterDelegate GetWriter(string pname) {
			SlotWriterDelegate d = null;
			if (pname != null) {
				string mname = string.Format("set_{0}", pname);
				MethodDefinition md = InnerBehavior.GetMethod(mname);
				if (md != null)
					d = delegate(object sender, string propName, object value) {
						return InnerBehavior.Invoke(mname, value);
					};
			}

			return d;
		}

		protected virtual void OnSlotError(string pname, object original, object value, Exception ex) {
			EventHandler<SlotErrorEventArgs> h = AfterSlotError;
			if (h != null) {
				SlotDefinition sd = GetSlot(pname);
				SlotErrorEventArgs args = (sd != null)
						? new SlotErrorEventArgs(ex, sd, value)
						: new SlotErrorEventArgs(ex, pname, value);
				args.OriginalValue = original;
				h(this, args);
			}
		}

		// TODO: �������� �������������� ����������!
		protected virtual bool OnBeforeSetSlotValue(string pname, object original, object value) {
			EventHandler<SlotChangeEventArgs> h = BeforeSetSlotValue;
			if (h != null) {
				SlotDefinition sd = GetSlot(pname);
				SlotChangeEventArgs args = (sd != null) 
						? new SlotChangeEventArgs(sd, value)
						: new SlotChangeEventArgs(pname, value);
				args.OriginalValue = original;
				h(this, args);
				return !args.Cancel;
			}
			return true;
		}

		protected virtual void OnAfterSetSlotValue(string pname, object original, object value) {
			EventHandler<SlotChangeEventArgs> h = AfterSetSlotValue;
			if (h != null) {
				SlotDefinition sd = GetSlot(pname);
				SlotChangeEventArgs args = (sd != null)
						? new SlotChangeEventArgs(sd, value)
						: new SlotChangeEventArgs(pname, value);
				args.OriginalValue = original;
				h(this, args);
			}
		}

		protected virtual void InitSlot(SlotDefinition slot) {
			// TODO: ������������... ���-�� �������
			if (slot == null)
				Error.Warning(new ArgumentNullException("slot"), typeof(LObject));
			else {
				if (RawGetSlotValue(slot.Name) == null) {
					object v = slot.Evaluate();
					if (v != null)
						RawSetSlotValue(slot.Name, v);
				}
			}
		}

		protected virtual void OnChangeType() {
			// TODO: ��������! ��������� ��������������, � ��������!

		}
		//................................................................
		#endregion
	}
	
	// TODO: � �������� ����� �������� ��� �������� � DefaultValue

	public delegate object SlotReaderDelegate(object sender, string pname);

	public delegate object SlotWriterDelegate(object sender, string pname, object value);

}
