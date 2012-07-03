// $Id: Name.cs 1417 2006-07-07 13:41:11Z sokol $

using System;
using System.Collections;
using System.Collections.Generic;

namespace Front {

	/// <summary>����� ��������� ��� ���� ��������� ��������.</summary>
	public interface INamed {
		/// <summary>��������� ������������� ������� ������������ �����.</summary>
		string Name { get; set; }
		/// <summary>����������������� ������������� ����� ���������.</summary>
		Name FullName { get; }
	}


	/// <summary>����������� ��������.</summary>
	public interface INamedValue<T> : INamed {
		T Value { get; set; }
	}


	public interface INamedValue : INamedValue<object> {
	}


	public interface ILabeled {
		string Label { get; set; }
	}

	/// <summary>���������������� ���.</summary>
	/// <remarks><para>��� ����� ������������ � ������������ ����, ����� ����� "�����" (�������) �  ��������� ������
	/// ��������� (<see cref="AliasList"/>).</para>
	/// <para>��� �� ���������������� �������� ����������� ����� (��������� ���� ��), ��� ���, ������ ���� - ��� ����
	/// <see cref="INamedValueCollection"/>.</para></remarks>
	// TODO DF0035: �������� ������������ � ��������� � �����.
	// TODO DF-19: Name ������ ������������� INamed! (����� �������. ���������)
	[Serializable]
	public class Name : INamed, Front.Data.IAsIsValue, ICloneable, IEquatable<Name>, IEquatable<string> {

		internal static Guid g = Guid.NewGuid();

		/// <summary>����������� ���.</summary>
		protected string InnerName = null;
		/// <summary>���������� ������ �� ���� �����.</summary>
		protected Name InnerBaseName = null;
		/// <summary>���������� ������ �� ������ ���������.</summary>
		protected WeakReference InnerAliasList = null;


		public Name(AliasList aliasList, Name baseName, string lastName) {
			AliasList = aliasList;
			InnerName = lastName;
			BaseName = baseName;
		}

		public Name(AliasList aliasList, params string[] nameParts) {
			AliasList = aliasList;
			if (nameParts.Length == 0) return;
			if (nameParts.Length == 1 && nameParts[0] != null && nameParts[0].IndexOf(".") > 0)
				nameParts = nameParts[0].Split('.');
			InnerName = nameParts[nameParts.Length - 1];
			if (nameParts.Length > 1) { // ���� �����
				string[] bs = new string[nameParts.Length - 1];
				for (int i = 0; i < bs.Length; i++) bs[i] = nameParts[i];
				BaseName = new Name(aliasList, bs);
			}
		}

		public Name(params string[] nameParts) : this((AliasList)null, nameParts) { }

		public Name(Name baseName, string lastName) : this((AliasList)null, baseName, lastName) { }


		/// <summary>��������� ������������� ������� ������������ �����.</summary>
		/// <remarks><para>�������������� �� ������� ������������ ����� <see cref="BaseName"/> 
		/// � ������������ ����� (<see cref="Name"/>).</para>
		/// <para>��� ��������� ������������ ���������<see cref="LastName"/>!</para></remarks>
		string INamed.Name {
			get { return (BaseName != null) ? (BaseName.OwnAlias + "." + this.LastName) : this.LastName; }
			set { this.LastName = value; }
		}

		/// <summary>��������� � ������������ ��������� �����.</summary>
		/// <remarks>������ this.Name</remarks>
		public string OwnAlias {
			get { return (BaseName != null) ? (BaseName.OwnAlias + "." + this.LastName) : this.LastName; }
			set { this.LastName = value; }
		}

		/// <summary>������ ����������� ���.</summary>
		/// <returns>���������� ������ �� ����.</returns>
		public virtual Name FullName {
			get { return this; }
		}

		/// <summary>����������� ���.</summary>
		// TODO DF-14: �������� ������� ��� ��������� 
		// TODO DF-15: ��� ��������� LastName ����� �������������� ���������� ������ � ������� (����� ��������� BaseName)
		public virtual string LastName {
			get { return InnerName; }
			set { InnerName = value; }
		}

		/// <summary>���� ����� (����� ���������� ��� ������������ ����).</summary>
		public virtual Name BaseName { get { return InnerBaseName; } set { InnerBaseName = value; } }

		/// <summary>���������� ��� �� ������� ����� ������� ����, ��� �������� BaseName ��������� c ��������� ������.</summary>
		public virtual Name ShiftTo(Name base_name) {
			Name res = this;
			while (res != null) {
				if (base_name != null && base_name.LastName != null && base_name.LastName != "") {
					if (res.BaseName != null && res.BaseName.Equals(base_name))
						return res;
				} else
					if (res.BaseName == null) return res;
				res = res.BaseName;
			}
			return res;
		}

		public virtual Name ShiftTo(string base_name) {
			return ShiftTo((base_name != null) ? new Name(base_name) : null);
		}

		public virtual Name ShiftBase(Name baseName) {
			string s = FullName.ToString().Replace(baseName.ToString() + ".", "");
			return s;
		}

		/// <summary>������ ��������� ����.</summary>
		/// <remarks>��� ����� ���� �������� � ���������� ������� ���������, �� ������� <see cref="Alias"/>
		/// ������� �� ����� ������.</remarks>
		public virtual AliasList AliasList {
			get {
				if (InnerAliasList != null && InnerAliasList.IsAlive) {
					AliasList a = InnerAliasList.Target as AliasList;
					if (!a.IsDisposed) return a;
				}
				return null;
			}
			set {
				if (value != null)
					InnerAliasList = new WeakReference(value);
				else
					InnerAliasList = null;
			}
		}

		/// <summary>������� �����.</summary>
		/// <remarks><para>���� ������ ��������� �� �����, �� ����� �������������� �� ������ <see cref="BaseName"/> 
		/// � ������������ ����� (<see cref="Name"/>).</para>
		/// </remarks>
		public virtual string Alias {
			get {
				AliasList x = AliasList;
				string last_name_alias = null;
				if (x != null) {
					string al = x.GetAlias(this.OwnAlias);
					if (al != null) return al;
					last_name_alias = x.GetAlias(this.LastName);
				}
				if (last_name_alias == null)
					last_name_alias = LastName;

				// TODO DF-17: ���������� Alias! � this.Name (������� �� DF-14)
				return (BaseName != null)
					? (BaseName.Alias + "." + last_name_alias) : last_name_alias;
			}
			set {
				AliasList al = this.AliasList;
				if (al == null)
					al = this.AliasList = new AliasList(this.OwnAlias);

				Name al_name = al.GetAlias(this.OwnAlias);
				if (al_name != null)
					al_name.Alias = value;
				else
					al.SetAlias(value, this.OwnAlias);
			}
		}

		/// <summary>������� ������ ����� �����.</summary>
		/// <remarks>������� ��� ��� �� �����������!</remarks>
		object ICloneable.Clone() { return this.Clone(); }

		/// <summary>������� ������ ����� �����.</summary>
		/// <remarks>������� ��� ��� �� �����������!</remarks>
		public virtual Name Clone() {
			return new Name(this.AliasList, ((BaseName != null) ? BaseName.Clone() : null), LastName);
		}

		/// <summary>���������� ����������� ����� � ������� ������������� ����.</summary>
		/// <returns>������ ���������� ���������� ��������� �������� ����� � ���������� ������ �����.</returns>
		/// <remarks><para>��������: 
		///		{ A.B.C | A.B.X.D } =>  1 { A.B }
		///		{ A.B.C.D | A.B.C.D.E } = 3 { A.B.C.D }
		///		{ A.B.C | X.Y } => -1 { }</para>
		/// <para>����� StartsWith("A.B.C") ���������������� � ����� StartsWith("A", "B", "C");</para>
		/// </remarks>
		// TODO DF-48: ���-�� ������� �������� � �� ����������������� � Name.StartsWith...
		public virtual int StartsWith(params string[] parts) {
			if (parts == null || parts.Length < 1) return -1;
			if (parts.Length == 1 && parts[0].IndexOf(".") > 0) return StartsWith(parts[0].Split('.'));

			int base_start = -1;

			if (BaseName != null) {
				base_start = BaseName.StartsWith(parts);
				if (base_start < 0) return base_start;
			}


			return (base_start + 1 < parts.Length && parts[base_start + 1] == InnerName)
				? (base_start + 1) : base_start;
		}

		/// <summary>���������� ����������� ����� � ������� ������������� ����.</summary>
		public string StartsWith(Name nspace) {
			ArrayList a = new ArrayList();
			while (nspace != null) {
				a.Insert(0, nspace.LastName);
				nspace = nspace.BaseName;
			}
			return SubName((string[])a.ToArray(typeof(string)));
		}

		/// <summary>��������� ������������� �����.</summary>
		/// <returns>���������� ������ � ��������� �����!</returns>
		public override string ToString() {
			// TODO DF-18: ������������ NameConverter!
			return this.Alias;
		}

		/// <summary>��������� ��������� ��������� ������������� ����.</summary>
		public virtual bool Equals(Name name) { return object.Equals(OwnAlias, name.OwnAlias); }

		/// <summary>��������� ��������� ���������� ������������� ����� � ��������� ������</summary>
		public virtual bool Equals(string name) { return object.Equals(OwnAlias, name); }

		/// <summary>�������� ������������ �������������� � ������ </summary>
		public static implicit operator string(Name n) { return (n != null) ? n.ToString() : null; }

		/// <summary>����������� ����� ��� ��������� �������� �����.</summary>
		/// <remarks>���� name ����� <c>null</c>, ������� ����� ���.</remarks>
		public static Name SetAlias(Name name, string alias) {
			if (name != null)
				name.Alias = alias;
			else
				name = new Name(alias);
			return name;
		}

		public static string[] SubName(int num, params string[] parts) {
			if (num < 0) return new string[] { };
			string[] res = new string[num + 1];
			for (int i = 0; i <= num; i++) res[i] = parts[i];
			return res;
		}

		public string SubName(params string[] parts) {
			if (parts == null || (parts.Length > 0 && parts[0] == null)) return "";
			if (parts.Length == 1 && parts[0].IndexOf(".") > 0)
				return SubName(parts[0].Split('.'));

			return String.Join(".", Name.SubName(StartsWith(parts), parts));
		}

		public static implicit operator Name(string s) {
			return new Name(s);
		}

		public override int GetHashCode() {
			return g.GetHashCode();
		}

	}


	/// <summary> ������� ������� ������������ ��������. </summary>
	public class NamedValue : INamedValue {
		protected Name InnerName;
		protected object InnerValue;

		public virtual string Name { get { return FullName.OwnAlias; } set { FullName.OwnAlias = value; } }
		public virtual Name FullName { get { return InnerName; } }
		public virtual object Value { get { return InnerValue; } set { InnerValue = value; } }


		public int GetInt32() { return Convert.ToInt32(Value); }
		public double GetDouble() { return Convert.ToDouble(Value); }
		public string GetString() { return InnerValue.ToString(); }
		public DateTime GetDateTime() { return Convert.ToDateTime(Value); }
		public bool GetBoolean() { return Convert.ToBoolean(Value); }
		public Decimal GetDecimal() { return Convert.ToDecimal(Value); }


		public NamedValue(string name, object value) : this(new Name(name), value) { }

		public NamedValue(Name name, object value) {
			InnerName = name;
			InnerValue = value;
		}
	}

	public class NamedValue<T> : INamedValue<T> {
		protected Name InnerName;
		protected T InnerValue;

		public virtual string Name { get { return FullName.OwnAlias; } set { FullName.OwnAlias = value; } }
		public virtual Name FullName { get { return InnerName; } }
		public virtual T Value { get { return InnerValue; } set { InnerValue = value; } }

		public int GetInt32() { return Convert.ToInt32(Value); }
		public double GetDouble() { return Convert.ToDouble(Value); }
		public string GetString() { return InnerValue.ToString(); }
		public DateTime GetDateTime() { return Convert.ToDateTime(Value); }
		public bool GetBoolean() { return Convert.ToBoolean(Value); }
		public Decimal GetDecimal() { return Convert.ToDecimal(Value); }


		protected NamedValue() {}

		public NamedValue(string name, T value) : this(new Name(name), value) { }

		public NamedValue(Name name, T value) {
			InnerName = name;
			InnerValue = value;
		}
	}
}
