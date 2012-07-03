// $Id: ProgressIndicator.cs 2421 2006-09-19 07:26:55Z kostya $

using System;

namespace Front.Diagnostics {

	// TODO DF0024: ���������� ������������� � ������� �������� ����� (��� ������������ ������).

	/// <summary>��������� ��������� ���������� ��������.</summary>
	/// <remarks>��������� �, �������������� ��������� ���������, ����� ������� ����������
	/// �������� �������� ����� Started, ������� Started ������ ������� ������-����, �������
	/// ����� ����� ������� ����������� � �� ��� ��������� ������ � ����� �������������
	/// ����������� <see cref="IProgressIndicator"/>, ��� ����, ����� �������� ���� �������� �������� �� ������.
	/// <para>�� ����� ���������� ��������, ��������� � ����� ����� �� ������� �������� �����
	/// Progress, � �������� ������� ��������� �������� ���������. ���� ��� ������ <see cref="Started"/>,
	/// <c>canProgress == false</c>, �� �������� percent ������ �� ����� (������ ����� ����� == 0).
	/// ��������� ������� Progress ��������� ���������� �, ����� �� ���������� �������� ��� ����
	/// �� ����������. ��, ���� ��� ������ <see cref="Started"/>, canAbort == false, �� ��������� � �����
	/// �� �����������.</para></remarks>
	public interface IProgressIndicator {

		/// <summary>�������� <see cref="IProgressIndicator"/> � ���������� ��������.</summary>
		/// <param name="source">��������� ����������� �������� (��� ����������� ��).</param>
		/// <param name="message">��������� ���������, ����������� ��������.</param>
		/// <param name="canProgress">���������� � ���, ����� �� ��������� � ��������� �������� ��������.</param>
		/// <param name="canAbort">���������� � ���, ����� �� ��� �������� ���������.</param>
		/// <returns>���� ������������������ ��������.</returns>
		/// <exception cref="ArgumentNullException">���� <b>source</b> ����� <c>null</c>.</exception>
		/// <remarks>����, ������� ���������� ��� �-�� ����� ���� ������� � ������ <see cref="Finished"/> ���
		/// �������� ����� ������ �������� �����������.<para> ���� <c>message</c> ����� null, ��� ������ ������
		/// <see cref="IProgressIndicator"/> ����� �������� ��������� �� ��������� �� ���������.</para></remarks>
		/// <seealso cref="Finished"/>
		/// <seealso cref="Progress"/>
		/// <seealso cref="HasActiveOperation"/>
		object Started(object source, string message, bool canProgress, bool canAbort);
		
		/// <summary>�������� <see cref="IProgressIndicator"/> � ���, ��� ���� �� ������������������ ��������
		/// ������� �����������.</summary>
		/// <param name="key">����, ���������� ��� ����������� ��������.</param>
		/// <param name="message">��������� ���������, �������������� ���������� ��������. ����� ���� ������ ������� ��� null.</param>
		/// <remarks>���� �������� � ��������������� ������ �� ���������������� (��������, ��� ��� ���������), �����
		/// ������������. </remarks>
		/// <seealso cref="Started"/>
		/// <seealso cref="Canceled"/>
		/// <seealso cref="Progress"/>
		void Finished(object key, string message);
		
		/// <summary>�������� <see cref="IProgressIndicator"/> � ���, ��� ���� �� ������������������ ��������
		/// �������� �������������.</summary>
		/// <param name="key">����, ���������� ��� ����������� ��������.</param>
		/// <param name="message">��������� ���������, �������������� ���������� ��������. ����� ���� ������ ������� ��� null.</param>
		/// <remarks>���� �������� � ��������������� ������ �� ���������������� (��������, ��� ��� ���������), �����
		/// ������������. </remarks>
		/// <seealso cref="Started"/>
		/// <seealso cref="Finished"/>
		/// <seealso cref="Progress"/>
		void Canceled(object key, string message);

		/// <summary>�������� <see cref="IProgressIndicator"/> � ��������� �������� � ������ � ���� ������� �� ����������
		/// ���������� ��������.</summary>
		/// <param name="key">����, ���������� ��� ����������� ��������.</param>
		/// <param name="percent">�������� �� 0 �� 100, ����������� ������� ������������� ��������.</param>
		/// <returns>���������� ��������, �����������, ����� �� ��������� ���������� �������� ��� �������
		/// ���������� �� ����������.</returns>
		/// <remarks>���� �������� � ��������������� ������ �� ���������������� (��������, ��� ��� ���������), �����
		/// ������������. <para>���� �������� ���� ���������������� ������� <see cref="Started"/> �� ���������
		/// ��������� <b>canAbort</b> ������ <c>false</c>, �� ��������� ����������� �������� ����� ���������������
		/// ������ ���������� ��������.</para></remarks>
		/// <seealso cref="Started"/>
		/// <seealso cref="Finished"/>
		/// <seealso cref="Message"/>
		bool Progress(object key, byte percent);

		/// <summary>������������ <see cref="IProgressIndicator"/> ����������� ������������ ���� ��������.</summary>
		/// <param name="key">����, ���������� ��� ����������� ��������.</param>
		/// <param name="message">��������� ���������, �������������� ���������� ��������. ����� ���� ������ ������� ��� null.</param>
		/// <remarks>���� �������� � ��������������� ������ �� ���������������� (��������, ��� ��� ���������), �����
		/// ������������. <para>������ �����, ��������� ���������� <see cref="IProgressIndicator"/> ����� �����-�� �������
		/// �������� ������������, �������� ���������� ������ ������� ����������.</para></remarks>
		/// <seealso cref="Started"/>
		/// <seealso cref="Finished"/>
		/// <seealso cref="Progress"/>
		void Message(object key, string message);

		/// <summary>������ ���� �� ������� ������������������ ��������.</summary>
		/// <value>true, ���� ���� ���� ���� ������������������ ��������; ����� false.</value>
		/// <seealso cref="Started"/>
		/// <seealso cref="Finished"/>
		bool HasActiveOperation { get; }
	}

	[Serializable]
	internal class NullIndicator : IProgressIndicator {
		object IProgressIndicator.Started(object source, string message, bool canProgress, bool canAbort) {
			return this;
		}
		void IProgressIndicator.Finished(object key, string message) { }
		void IProgressIndicator.Canceled(object key, string message) { }
		bool IProgressIndicator.Progress(object key, byte percent) { return true; }
		void IProgressIndicator.Message(object key, string msg) { }
		bool IProgressIndicator.HasActiveOperation { get { return false; } }
	}

	/// <summary>����������� �����, ����������� ����� ��������� ��������� ��������� <see cref="IProgressIndicator"/>
	/// �� ���������� <see cref="IServiceProvider"/> � ������� ���� �� ��� �������.</summary>
	/// <remarks>���� ����������� ������� ��������� ������� <see cref="IProgressIndicator"/> ������, ������ ���
	/// ����� �� ����� ������ ����� ������ ����� �������� � ������������� �������� ��������. <see cref="ProgressIndicator"/>
	/// ��������� ����� ������� <see cref="IProgressIndicator"/> � <see cref="IServiceProvider"/> ��� ������
	/// ������. ��� �������� ������������������ � ����� �������� � ������������ ���������, � ������ ��������� ������������
	/// � <see cref="IServiceProvider"/> ����� ����� �������� <see cref="ProgressIndicator"/>.
	/// <para>�������� �������� ������������� ���������� <see cref="IProgressIndicator"/> ��� ����� ��������� ����������.</para></remarks>
	public sealed class ProgressIndicator {
		static IProgressIndicator nullIndicator;
		

		/// <summary>���������� �����������, ����������� �������� ���������� ������ <see cref="ProgressIndicator"/></summary>
		ProgressIndicator() {}
		
		/// <summary>�������� �������� <see cref="IProgressIndicator"/> � ���������� ��������.</summary>
		/// <param name="sp"><see cref="System.IServiceProvider"/>, ������������ ��� ������ �������� <see cref="IProgressIndicator"/>.</param>
		/// <param name="source">��������� ����������� �������� (��� ����������� ��).</param>
		/// <param name="message">��������� ���������, ����������� ��������.</param>
		/// <param name="canProgress">���������� � ���, ����� �� ��������� � ��������� �������� ��������.</param>
		/// <param name="canAbort">���������� � ���, ����� �� ��� �������� ���������.</param>
		/// <returns>���� ������������������ ��������.</returns>
		/// <exception cref="ArgumentNullException">���� <b>source</b> ����� <c>null</c>.</exception>
		/// <remarks>����, ������� ���������� ��� �-�� ����� ���� ������� � ������ <see cref="Finished"/> ���
		/// �������� ����� ������ �������� �����������.<para> ���� <c>message</c> ����� null, ��� ������ ������
		/// <see cref="IProgressIndicator"/> ����� �������� ��������� �� ��������� ��-���������.</para></remarks>
		/// <seealso cref="Finished"/>
		/// <seealso cref="Progress"/>
		public static object Started(IServiceProvider sp, object source, string message, bool canProgress, bool canAbort) {
			return GetIndicator(sp).Started(source, message, canProgress, canAbort);
		}
		
		/// <summary>�������� �������� <see cref="IProgressIndicator"/> � ���, ��� ���� �� ������������������ ��������
		/// ������� �����������.</summary>
		/// <param name="sp"><see cref="System.IServiceProvider"/>, ������������ ��� ������ �������� <see cref="IProgressIndicator"/>.</param>
		/// <param name="key">����, ���������� ��� ����������� ��������.</param>
		/// <remarks>���� �������� � ��������������� ������ �� ���������������� (��������, ��� ��� ���������), �����
		/// ������������. </remarks>
		/// <seealso cref="Started"/>
		/// <seealso cref="Canceled"/>
		/// <seealso cref="Progress"/>
		public static void Finished(IServiceProvider sp, object key) {
			GetIndicator(sp).Finished(key, null);
		}

		/// <summary>�������� �������� <see cref="IProgressIndicator"/> � ���, ��� ���� �� ������������������ ��������
		/// ������� �����������.</summary>
		/// <param name="sp"><see cref="System.IServiceProvider"/>, ������������ ��� ������ �������� <see cref="IProgressIndicator"/>.</param>
		/// <param name="key">����, ���������� ��� ����������� ��������.</param>
		/// <param name="message">��������� ���������, �������������� ���������� ��������. ����� ���� ������ ������� ��� null.</param>
		/// <remarks>���� �������� � ��������������� ������ �� ���������������� (��������, ��� ��� ���������), �����
		/// ������������. </remarks>
		/// <seealso cref="Started"/>
		/// <seealso cref="Canceled"/>
		/// <seealso cref="Progress"/>
		public static void Finished(IServiceProvider sp, object key, string message) {
			GetIndicator(sp).Finished(key, message);
		}
		
		/// <summary>�������� �������� <see cref="IProgressIndicator"/> � ���, ��� ���� �� ������������������ ��������
		/// �������� �������������.</summary>
		/// <param name="sp"><see cref="System.IServiceProvider"/>, ������������ ��� ������ �������� <see cref="IProgressIndicator"/>.</param>
		/// <param name="key">����, ���������� ��� ����������� ��������.</param>
		/// <remarks>���� �������� � ��������������� ������ �� ���������������� (��������, ��� ��� ���������), �����
		/// ������������. </remarks>
		/// <seealso cref="Started"/>
		/// <seealso cref="Finished"/>
		/// <seealso cref="Progress"/>
		public static void Canceled(IServiceProvider sp, object key) {
			GetIndicator(sp).Canceled(key, null);
		}
		
		/// <summary>�������� �������� <see cref="IProgressIndicator"/> � ���, ��� ���� �� ������������������ ��������
		/// �������� �������������.</summary>
		/// <param name="sp"><see cref="System.IServiceProvider"/>, ������������ ��� ������ �������� <see cref="IProgressIndicator"/>.</param>
		/// <param name="key">����, ���������� ��� ����������� ��������.</param>
		/// <param name="message">��������� ���������, �������������� ���������� ��������. ����� ���� ������ ������� ��� null.</param>
		/// <remarks>���� �������� � ��������������� ������ �� ���������������� (��������, ��� ��� ���������), �����
		/// ������������. </remarks>
		/// <seealso cref="Started"/>
		/// <seealso cref="Finished"/>
		/// <seealso cref="Progress"/>
		public static void Canceled(IServiceProvider sp, object key, string message) {
			GetIndicator(sp).Canceled(key, message);
		}
		
		/// <summary>�������� �������� <see cref="IProgressIndicator"/> � ��������� �������� � ������ � ���� ������� �� ����������
		/// ���������� ��������.</summary>
		/// <param name="sp"><see cref="System.IServiceProvider"/>, ������������ ��� ������ �������� <see cref="IProgressIndicator"/>.</param>
		/// <param name="key">����, ���������� ��� ����������� ��������.</param>
		/// <param name="percent">�������� �� 0 �� 100, ����������� ������� ������������� ��������.</param>
		/// <returns>���������� ��������, �����������, ����� �� ��������� ���������� �������� ��� �������
		/// ���������� �� ����������.</returns>
		/// <remarks>���� �������� � ��������������� ������ �� ���������������� (��������, ��� ��� ���������), �����
		/// ������������. <para>���� �������� ���� ���������������� ������� <see cref="Started"/> �� ���������
		/// ��������� <b>canAbort</b> ������ <c>false</c>, �� ��������� ����������� �������� ����� ���������������
		/// ������ ���������� ��������.</para></remarks>
		/// <seealso cref="Started"/>
		/// <seealso cref="Finished"/>
		/// <seealso cref="Message"/>
		public static bool Progress(IServiceProvider sp, object key, byte percent) {
			return GetIndicator(sp).Progress(key, percent);
		}

		/// <summary>������������ �������� <see cref="IProgressIndicator"/> ����������� ������������ ���� ��������.</summary>
		/// <param name="sp"><see cref="System.IServiceProvider"/>, ������������ ��� ������ �������� <see cref="IProgressIndicator"/>.</param>
		/// <param name="key">����, ���������� ��� ����������� ��������.</param>
		/// <param name="message">��������� ���������, �������������� ���������� ��������. ����� ���� ������ ������� ��� null.</param>
		/// <remarks>���� �������� � ��������������� ������ �� ���������������� (��������, ��� ��� ���������), �����
		/// ������������. <para>������ �����, ��������� ���������� <see cref="IProgressIndicator"/> ����� �����-�� �������
		/// �������� ������������, �������� ���������� ������ ������� ����������.</para></remarks>
		/// <seealso cref="Started"/>
		/// <seealso cref="Finished"/>
		/// <seealso cref="Progress"/>
		public static void Message(IServiceProvider sp, object key, string message) {
			GetIndicator(sp).Message(key, message);
		}

		/// <summary>�������� ��������� �������, �������������� ��������� <see cref="IProgressIndicator"/> � ������������
		/// ��� ������.</summary>
		/// <value>��������� ����������� ������ <b>NullIndicator</b>, ������� ��������� ��� ������ <see cref="IProgressIndicator"/>.</value>
		/// <remarks>���������� ������ <b>NullIndicator</b> ������, ��� �� ���������� ��� ������. �� �� ������� ���������� �
		/// ����� ���� ����������� � ���� �������� �� ���� ������, ��� ��������� ��������� <see cref="IProgressIndicator"/>,
		/// � ������, ���� ���� ���������������� <see cref="IProgressIndicator"/> �� �����.</remarks>
		/// <seealso cref="Started"/>
		/// <seealso cref="Finished"/>
		/// <seealso cref="Progress"/>
		public static IProgressIndicator NullIndicator {
			get {
				if (nullIndicator == null) nullIndicator = new NullIndicator();
				return nullIndicator;
			}
		}

		/// <summary>�������� ������� <see cref="IProgressIndicator"/>.</summary>
		/// <returns><see cref="IProgressIndicator"/>, ������� ��������� � ��������� <see cref="IServiceProvider"/> ���
		/// <c>NullIndicator</c>, ���� ������ <see cref="IProgressIndicator"/> �� ���������.</returns>
		public static IProgressIndicator GetIndicator(IServiceProvider sp) {
			IProgressIndicator pi = sp.GetService(typeof(IProgressIndicator)) as IProgressIndicator;
			return (pi == null) ? NullIndicator : pi;
		}
	}
}
