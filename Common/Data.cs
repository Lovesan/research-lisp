// $Id: Data.cs 899 2006-06-06 16:09:40Z john $


using System;
using System.Collections;
using System.Data;
using System.Text;
using System.Collections.Generic;

using Front.Converters;

namespace Front.Data {

	/// <summary>��������� ����-���������� � ������� � �������������� <see cref="DataSet"/>.</summary>
	public interface ISchemeReader {
		void ReadScheme(DataSet ds, string tname);
	}


	/// <summary>
	/// <see cref="IAsIsValue"/>. ��� ������� ������� �� ��������� ������
	/// �������� �������� ����� <see cref="IValueConverter"/> � ��������������.
	/// ���� ����� ���������� ����� <see cref="IValueConverter"/> ������-��������,
	/// ������� �� ����� �������������� �������� ��� �� ���� ������-��������
	/// ���������� ��������� <see cref="IAsIsValue"/>.
	/// </summary>
	/// <remarks>
	/// <para> </para>
	/// </remarks>
	public interface IAsIsValue {
	}


	public interface IExpr : ICloneable{
		IList<Operand> Operands { get; }
	}


	[Serializable]
	public class Operand {
		public Operand() {}
		public Operand(object o) {
			Value = o;
		}
		public object Value;
	}

	
	/// <summary>�������� � �������. ������ ���� @0 @1 @2... ������ - ��� ������� ������.</summary>
	/// <remarks>��� ��������� ������ �� ������� - ����� ���������� Security hole!</remarks>
	[Serializable]
	public class QueryParameter : IAsIsValue, IExpr {
		protected string m_p = null;
		protected Operand m_o;
		public DbType DbType = DbType.Object;
		public int Size = 0;
			
		public string Parameter { get { return m_p; } set { m_p = value; } }
		public IList<Operand> Operands { 
			get { 
				IList<Operand> res = new List<Operand>();
				res.Add(m_o);
				return res;
			}
		}

		public object Value { 
			get { return m_o.Value; } 
			set { m_o.Value = value; }
		}

        /// <summary>
        /// Constructor for XmlSerialization
        /// </summary>
        protected QueryParameter(){}

		public QueryParameter(string pname, object value) : this(pname, new Operand( value )) { }
		public QueryParameter(string pname) : this (pname, new Operand()) { }
		public QueryParameter(string pname, Operand operand) { 
			m_p = pname; 
			m_o = operand;
		}

		public override string ToString() { return m_p; }

		// XXX � ����� ���� m_o ���� ����� �����������?
		public object Clone() {
			return new QueryParameter(m_p, m_o);
		}
	}

	/// <summary>�������� �������, ������� �������� � ���� ��� ������� �����.</summary>
	/// <remarks>��������� ���������� � ������ ����� ������, ����� � ������ ��������.</remarks>
	/// <seealso cref="Front.Data"></seealso>
	[Serializable]
	public class NameParameter : IAsIsValue {
		protected Name nm;
		public virtual Name Name { get { return nm; } }

		public NameParameter(Name p) {
			this.nm = p;
		}
		
		/// <summary>Name ������������� � ������, ��������� ������������� <see cref="NameConverter"/>. </summary>
		public override string ToString() {
			return GenericConverter.String(Name);
		}
	}

	/// <summary> ������������� ��������� ���� ������ </summary>
	public interface IDBContext {
		string DbmsName { get; }
	}


	public class MSSQLDBContext : IDBContext {
		// TODO: ��� ������� ���� �� ������ � �����������: MSDE, Server, Enterprise � �.�.
		public string DbmsName { get { return "MsSQL"; } }

		public static ContextSwitch<IDBContext> ON {
			get {
				return new ContextSwitch<IDBContext>(new MSSQLDBContext());
			}
		}

		public static bool IsOn {
			get {
				IDBContext x = ContextSwitch<IDBContext>.Current;
				return (x != null && typeof(MSSQLDBContext) == x.GetType());
			}
		}
	}


	public class OracleDBContext : IDBContext {
		public string DbmsName { get { return "Oracle"; } }

		public static ContextSwitch<IDBContext> ON {
			get {
				return new ContextSwitch<IDBContext>(new OracleDBContext());
			}
		}
		public static bool IsOn {
			get {
				IDBContext x = ContextSwitch<IDBContext>.Current;
				return (x != null && typeof(OracleDBContext) == x.GetType());
			}
		}

	}

}
