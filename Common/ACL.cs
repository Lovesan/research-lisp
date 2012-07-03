// $Id: ACL.cs 128 2006-04-06 11:55:30Z pilya $


using System;
using System.Collections;
using System.Collections.Specialized;

namespace Front.Security {

	/// <summary>����� ����� <see cref="Acl"/> ���������� ������ ���������� ��� ������� ����������� �����,
	/// �� ���������� �������� <see cref="Permission"/> ��� �������� ���� ����� ������ �������� ����� �����
	/// ��� ����������� SID-�.</summary>
	/// <remarks>����� ���������� ������� ���������� ����� � ��������� SID-�� ������� �� 2-� ��������:
	/// <para>1. ��� �� ��������� - �� ���������.</para>
	/// <para>2. ������ ����� ������������ ��� �����������.</para>
	/// <para>��� �������� ��� ���� � ����������� ��������� SID-�� ������������ ��� ������ ��� ������ SID-�,
	/// ��� �������� � <see cref="Acl"/> ���-�� ���������� ����� ������������ �����, �� � ���������� ���������, ���
	/// ���� ������������ ��� ������ ������ ����� �� �����. ���� �� � <see cref="Acl"/> ���� SID-�, ���������������
	/// ���������, �� ����� ����� ��������� ������ � ��� ������, ���� ����� ������������������
	/// �������� ����� ���� ��� ������ <see cref="Permission.Allow"/> � �� ����� <see cref="Permission.Deny"/>.</para>
	/// </remarks>
	[Serializable]
	public enum Permission {
		///<summary>�������� ��� ������� SID-� �� ��������.</summary>
		Unspecified = 30,
		///<summary>����� ���������.</summary>
		Allow = 20,
		///<summary>����� ���������.</summary>
		Deny = 10
	}
	
	/// <summary>������ ���� �������.</summary>
	/// <remarks>���� ����� �� ���������� ������ ��������� ����. ������ ����� ����� �������� ����������
	/// ����������������, ��� ���� ����������� ������������ ������ ������ ���������� ��������� ���� �������.
	/// </remarks>
	[Serializable]
	public class Acl : ICloneable {
		HybridDictionary rights;

		/// <summary>������� ����� ������ ������ ���� �������.</summary>
		public Acl():this(new HybridDictionary()) { }

		protected Acl(HybridDictionary hd) {
			rights = hd;
		}

		object ICloneable.Clone() {
			return this.Clone();
		}

		public virtual Acl Clone() {
			HybridDictionary hd = new HybridDictionary(rights.Count);
			foreach (DictionaryEntry de in rights)
				if (de.Value != null) hd.Add(de.Key, ((ArrayList)de.Value).Clone());
			return new Acl(hd);
		}

		/// <summary>�������� ��������, ����������� ����� �� ������ ��������� SID-�� ��������� �����.</summary>
		/// <value>true, ���� � ��������� ��������� SID-�� ���� ������ �����; ����� false.</value>
		/// <remarks>����� ���������� ������� ���������� ����� � ��������� SID-�� ������� �� 2-� ��������:
		/// <para>1. ��� �� ��������� - �� ���������.</para>
		/// <para>2. ������ ����� ������������ ��� �����������.</para>
		/// <para>��� �������� ��� ���� � ��������� <paramref name="sids"/> ��� ������ SID-�, ��� ��������
		/// � <see cref="Acl"/> ���-�� ���������� ����� <paramref name="accessType"/>, �� � ���������� ���������, ���
		/// ���� ������ SID-�� ������ ����� �� �����. ���� �� � <see cref="Acl"/> ���� SID-�, ���������������
		/// <paramref name="sids"/>, �� ����� ����� ��������� ������ � ��� ������, ���� ����� ������������������
		/// �������� ����� ���� �� ���� <see cref="Permission.Allow"/> � �� ����� <see cref="Permission.Deny"/>.</para>
		/// </remarks>
		/// <param name="accessType">��� �������, ��� �������� ������� ��������� ����������.</param>
		/// <param name="sids">��������� SID-��, ��� ������� ����� ������������� ����������. ���� �����, ���
		/// SID ������������ � SID-� ���� �����, � ������� ������������ ������.</param>
		public bool this[string accessType, ICollection sids] {
			get {
				if (accessType == null) throw new ArgumentNullException("accessType");
				if (sids == null) throw new ArgumentNullException("sids");
				ArrayList p = rights[accessType] as ArrayList;

				if (p != null) {
					ArrayList sidList = new ArrayList(sids);
					sidList.Sort();

					bool bAllow = false;
					bool bDeny = false;

					// scan permission items
					foreach(AcItem cp in p)
						if (sidList.BinarySearch(cp.Sid) >= 0) {
							if (cp.Permission == Permission.Deny) {
								bDeny = true; break;
							}
							if (cp.Permission == Permission.Allow)
								bAllow = true;
						}
					return bAllow && !bDeny;
				} else
					return false;
			}
		}

		/// <summary>���������������� ��� ����� <paramref name="accessType"/> � ���������� SID-�
		/// ���������� <paramref name="permission"/>.</summary>
		/// <param name="sid">SID, ��� �������� �������������� �����</param>
		/// <param name="accessType">����� (��� �������)</param>
		/// <param name="permission">��������.</param>
		/// <remarks>���� ��������������� ����� ��� �����������������, ��� ����� ������������.
		/// <para>���� �������� <paramref name="permission"/> ����� <see cref="Permission.Unspecified"/>, �� �����������
		/// ���������.</para>
		/// </remarks>
		public void SetPermission(int sid, string accessType, Permission permission) {
			// get configured sids
			ArrayList list = rights[accessType] as ArrayList;
			if (list == null) list = new ArrayList();

			AcItem n = new AcItem(sid, permission);
			//
			// change occurrence of sid in list if exists
			int index = list.BinarySearch(n, new SidComparer());
			if (index >= 0) {
				if (permission == Permission.Unspecified)
					list.RemoveAt(index);				
				else {
					list[index] = n;
					list.Sort();
				}
			} else {
				list.Add(n);
				list.Sort();
			}
			// save to Acl for this access type
			rights[accessType] = list;
		}

		/// <summary>�������� ������� SID-��, ��� ������� ���������������� ���� �����-�� �����.</summary>
		/// <returns>������ SID-��.</returns>
		public int[] GetConfiguredSids() {
			ArrayList temp = new ArrayList();
			foreach (ArrayList a in rights.Values)
				temp.AddRange(a);
			temp.Sort();
			ArrayList res = new ArrayList();
			AcItem prev = new AcItem(-1, Permission.Unspecified); // create stub permission
			foreach(AcItem item in temp) {
				if (prev.Sid != item.Sid)
					res.Add(item.Sid);
				prev = item;
			}
			return (int[])res.ToArray(typeof(int));
		}

		/// <summary>�������� ������ ����, ������� ���������������� � <see cref="Acl"/>.</summary>
		/// <returns>������ ����� - ������ ������������������ ����.</returns>
		public string[] GetConfiguredRights() {
			string[] res = new string[rights.Count];
			rights.CopyTo(res, 0);
			return res;
		}
		
		/// <summary>�������� ������ SID-��, ��� ������� ���������������� ���������� �����.</summary>
		/// <returns>������� <see cref="IDictionary"/>, ������� �������� ���� "SID"-"��������" ��� ���������� �����.</returns>
		/// <remarks>���� �������������� ����� ��� ������-�� SID-� ����� �������� <see cref="Permission.Unspecified"/>, �� 
		/// ���� ����� ����� ������� ��������� SID �� ��������� <see cref="Permission.Unspecified"/> ��� ��
		/// ���������� ��� ������.</remarks>
		/// <param name="right">��� ������� (�����).</param>
		public IDictionary GetSids(string right) {
			IDictionary res = new Hashtable();
			ArrayList a = (ArrayList)rights[right];
			if (a != null) {
				foreach (AcItem item in a)
					res[item.Sid] = item.Permission;
			}
			return res;
		}

		/// <summary>�������� ������ ����, ������� ���������������� ��� ����������� SID-�.</summary>
		/// <returns>������� <see cref="IDictionary"/>, ������� �������� ���� "�����"-"��������" ��� ���������� SID-�.</returns>
		/// <remarks>���� �������������� ����� ��� ������-�� SID-� ����� �������� <see cref="Permission.Unspecified"/>, �� 
		/// ���� ����� ����� ������� ��������� ����� �� ��������� <see cref="Permission.Unspecified"/> ��� ��
		/// ���������� ��� ������.</remarks>
		/// <param name="sid">Security Identifier (SID).</param>
		public IDictionary GetRights(int sid) {
			IDictionary res = new ListDictionary();
			foreach (string key in rights.Keys) {
				ArrayList a = (ArrayList)rights[key];
				int index = -1;
				for (int ind=0; ind < a.Count; ind++)
					if (((AcItem)a[ind]).Sid == sid) {
						index = ind;
						break;
					}
				if (index < 0)
					res[key] = Permission.Unspecified;
				else 
					res[key] = ((AcItem)a[index]).Permission;
			}
			return res;			
		}

		[Serializable]
		internal class AcItem : IComparable {
			public readonly int			Sid;
			public readonly Permission	Permission;

			public AcItem(int sid, Permission p) {
				this.Sid = sid;
				this.Permission = p;
			}

			public int CompareTo(object obj) {
				AcItem other = (AcItem)obj;
				int res = this.Sid.CompareTo(other.Sid);
				if (res == 0)
					return this.Permission.CompareTo(other.Permission);
				else
					return res;
			}

			public override bool Equals(object obj) {
				if (obj is int) 
					return object.Equals(this.Sid, (int)obj);
				else
					return (this.CompareTo(obj) == 0);
			}

			public override int GetHashCode() {
				return Sid * (int)Permission;
			}
		}

		internal class SidComparer : IComparer {
			public int Compare(object x, object y) {
				AcItem a = (AcItem)x;
				AcItem b = (AcItem)y;
				return a.Sid.CompareTo(b.Sid);

			}
		}
	}
}

