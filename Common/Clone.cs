using System;
using System.Data;
using System.Reflection;
using System.Collections;
using System.Runtime.Remoting.Proxies;

namespace Front {



	public class GenericClone {

		// XXX �� ��� ��� ��� ������� � ����������� ���������.....
		// TODO: � ����� ������� ���� ��� Thread Safe
		public static object Clone(object o) {
			if (o == null) return null;
			
			Hashtable sl = ContextSwitch<Hashtable>.GetCurrent("Front.GenericClone:CloneList");
			if (sl == null) {
				using (GenericClone.StartClone) {
					// ��������� ������ � ��������� ������.
					return Clone(o);
				}
			} else {
				Type t = o.GetType();

				if (t.IsValueType) return o;

				object res = sl[o];
				if (res == null) {
				// ������ ����� �� ������������!

					// � ������ ����������� ������ ������� � ���, ��� ����������� ���� ������!
					// TODO: ��� ����� ����� ���������� ��� ������ �������, �����
					// ����������� ��������� ����� ������������ �� ���������!
					if (sl.ContainsKey(o)) return null;
					sl[o] = null;


					if ((o is MarshalByRefObject) || (o is RealProxy) || (o is WeakReference)) {
						// ������� ���� ����� �� �����������!
						return o;

					} else if (o is ICloneable) {
						// TODO: ����� ����� ����������� ��������� ��������� ������������ 
						// ��� ��������� ICloneable,����� ��� ArrayList, ������� �� ��������� ���� ��������!
						res = ((ICloneable)o).Clone();
 
					} else {
						
						MethodInfo mi = t.GetMethod("Clone", 
							BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic |
							BindingFlags.IgnoreCase | BindingFlags.FlattenHierarchy, null, new Type[0], null);

						if (mi != null) {
							try {
								res = mi.Invoke(o, null);
							} catch (Exception ex) {
								// ��������, ��� ��� ����� ����������?
							}
						} else {
							// ���������� ����� MemberwiseClone....
							// �� �� ��������� ��������� ���������!
							mi = t.GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic);
							res = mi.Invoke(o, null);
						}
					}
					sl[o] = res;
				}
				return res;
			}	
		}

		public static ContextSwitch<Hashtable> StartClone { 
			get {
				// ��� ������� ��������� Using(StartClone), ����� ������������ ��� ���������!
				if (ContextSwitch<Hashtable>.GetCurrentSwitch("Front.GenericClone:CloneList") == null)
					return new ContextSwitch<Hashtable>(new Hashtable(), "Front.GenericClone:CloneList");
				else
					return new ContextSwitch<Hashtable>(null, "Front.GenericClone.SecondaryClone");
			}
		}

	}
}
