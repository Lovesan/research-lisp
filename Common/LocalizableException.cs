// $Id: LocalizableException.cs 1836 2006-07-26 11:00:47Z kostya $

using System;
using System.Collections;
using System.Resources;
using System.Reflection;
using System.Runtime.Serialization;
using System.Globalization;
using System.Text;

namespace Front {

	/// <summary>��������� <see cref="Exception"/>, ������� ����� � ����������� � �������������� ���������.</summary>
	/// <remarks>����� <see cref="LocalizableException"/>, ������������ ��� ������ � �������������� ��������,
	/// � ������, ���� ���������� ����������� ������ ����������� ����������������� ��������.<para>
	/// �������� ������������ � ���, ��� ������, ���������������, ��������, � ������������ ���������, ��� �������� �
	/// �������������, ����� ���� �������� �� ������� �����, ��� ������� ��������������� ��������.</para>
	/// <para>��� ������������ ������ <see cref="LocalizableException"/> � ������� �������� �� <b>Front.Common</b>,
	/// ���������� ������������� ����� <see cref="LoadString"/>.</para></remarks>
	[Serializable]
	public class LocalizableException : Exception {
		protected string InnerErrorCode;
		protected object[] InnerArguments;

		/// <summary>��� ������.</summary>
		public string ErrorCode { get { return InnerErrorCode; } }
		/// <summary>��������� ��� ����������� � �������������� ������ ���������.</summary>
		public object[] Arguments { get { return InnerArguments; } }

		/// <summary>�������������� ����� ��������� <see cref="LocalizableException"/>.</summary>
		/// <remarks>���� ����������� �������������� <see cref="ErrorCode"/> ��������� <b>EUnknownError</b></remarks>
		public LocalizableException() : this("EUnknownError") { }

		/// <summary>�������������� ����� ��������� <see cref="LocalizableException"/> ��������� 
		/// ����� ������ � �����������.</summary>
		/// <param name="errorCode">��� ������.</param>
		/// <param name="args">��������� ��� ����������� � �������������� ��������� �� ������.</param>
		/// <remarks> ���� <paramref name="errorCode"/> ����� null (Nothing � VB.NET) ��� ������ ������, 
		/// �� ����� ������� �������� EUnknownError.</remarks>
		public LocalizableException( string errorCode, params object[] args )
			: this(null, errorCode, args)
		{ }

		/// <summary>�������������� ����� ��������� <see cref="LocalizableException"/> ��������� ����� ������,
		/// �����������, � ��������� �����������.</summary>
		/// <param name="inner">��������� ����������.</param>
		/// <param name="errorCode">��� ������.</param>
		/// <param name="args">��������� ��� ����������� � �������������� ��������� �� ������.</param>
		/// <remarks> ���� <paramref name="errorCode"/> ����� null (Nothing � VB.NET) ��� ������ ������, 
		/// �� ����� ������� �������� EUnknownError.</remarks>
		public LocalizableException( Exception inner, string errorCode, params object[] args )
			: base(null, inner)
		{
			this.InnerErrorCode = errorCode;
			this.InnerArguments = args;
		}

		/// <summary>�������������� ����� ��������� ����������. ���������� ������� <see cref="ISerializable"/>.</summary>
		protected LocalizableException(SerializationInfo info, StreamingContext context)
			: base(info, context) 
		{
			this.InnerErrorCode = info.GetString("ErrorCode");
			this.InnerArguments = info.GetValue("Arguments", typeof(object[])) as object[];
		}

		public override void GetObjectData( SerializationInfo info, StreamingContext context ) 
		{
			base.GetObjectData(info, context);
			info.AddValue("ErrorCode", InnerErrorCode);
			info.AddValue("Arguments", InnerArguments);
		}

		/* TODO DF0003: ��������� � �������� ����!
		/// <summary>��������� ��� ������.</summary>
		/// <value>��� ������.</value>
		/// <remarks>��� ������ ��� ������, ���������� ��� ���������� ������� � ��������� ��������� ������.
		/// ��� ������ �������� � ������ ����������� ���������� <see cref="LocalizableException"/>.</remarks>
		public virtual string ErrorCode {
			get {
				return (this.Message == null || this.Message.Length == 0) ? "EUnknownError" : base.Message;
			}
		}
		*/

		/// <summary>��������� ��������� ��������� �� ������.</summary>
		/// <value>��������� ��������� �� ������.</value>
		/// <remarks>��������� �� ������ �� ���������� ��� ������������ ����������. ����� �������, ����
		/// ������-�������� ���������� � ������-���������� �������� � ���������� � ���������� �����������
		/// ������� ��������, ����������� ����� ������������ ������ ���������, �������������� ���
		/// ��������� ����������.
		/// <para>��� ������������ ������ <see cref="LocalizableException"/> � ������� �������� �� <b>Front.Common</b>,
		/// ���������� �������������� ����� <see cref="FormatLocalizedMessage"/>.</para>
		/// </remarks>
		public override string Message {
			get {
				//return String.Format(ErrorCode, Arguments);
				// ���� ���������, ���� ����� �������� ������ � ���������
				string localized = FormatLocalizedMessage(ErrorCode, Arguments);
				return (localized == null) ? ErrorCode : localized;
			}
		}

		/// <summary>��������������� ��������� ��������� ������� ��������.</summary>
		/// <returns>����������������� ������������� ��������� ��������� �� ������.</returns>
		/// <remarks>���� ����� �������� ��� ������ � ��������� ��������� �������� ������
		/// (<c>Thread.CurrentUICulture</c>) ������ ������� � ������� �������������� ������
		/// ������� ���������. ��� ��� ���������� <see cref="LocalizableException"/> ������ ������
		/// ������ ������� � ����� ��������, ���� ����� ���������� �������������� � �����������, ����
		/// ��� ���������� ����������� ���� ������.</remarks>
		/// <param name="errorCode">��� ������ ��� ��������������.</param>
		/// <param name="args">��������� ��� ����������� � ��������� ���������.</param>
		protected string FormatLocalizedMessage( string errorCode, params object[] args ) 
		{
			string s = LoadString(errorCode);
			return String.Format( ((s == null) ? errorCode : s), args);
		}

		/// <summary>��������� �� �������� ������, � ������� ��������� ������ ����� ������ �
		/// ��������� ��������������� � ������� ��������� UI.</summary>
		/// <returns>������ � ������� UI ��������, ��� null, ���� ������ � ����� ��������������� �� �������.</returns>
		/// <remarks>���� ����� �������� ��� ������ � ��������� ��������� �������� ������
		/// (<c>Thread.CurrentUICulture</c>) ������ ������� � ������� �������������� ������
		/// ������� ���������. ��� ��� ���������� <see cref="LocalizableException"/> ������ ������
		/// ������ ������� � ����� ��������, ���� ����� ���������� �������������� � �����������, ����
		/// ��� ���������� ����������� ���� ������.</remarks>
		/// <param name="code">��� ������ ��� ��������������.</param>
		protected virtual string LoadString(string code) {
			return RM.GetString(code);
		}

		public static string PackExceptionInfo(Exception ex, bool includeStack) {
			StringBuilder sb = new StringBuilder();
			while (ex != null) {
				sb.AppendFormat("{0}\n", (includeStack ? ex.ToString() : ex.Message));
				ex = ex.InnerException;
			}
			return sb.ToString();
		}
	}
}
