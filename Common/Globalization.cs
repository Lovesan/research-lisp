//$Id: Globalization.cs 129 2006-04-06 12:00:46Z pilya $

using System;
using System.Threading;
using System.Globalization;

namespace Front.Globalization {

	//TODO DF0022: ������������ ContextSwitch<T>
	/// <summary>���������� UI ��������.</summary>
	/// <remarks>�������� �� ������ ������� �������� ������ � ������������ �������������� ������,
	/// ���������� �����������. <see cref="UICulturePublisher"/> ��������� ��� ������. �������� ������.
	/// </remarks>
	/// <example><code>
	///	try {
	/// 	// ��� ������������� ��������, �������� en-US
	///		using (new UICulturePublisher("ru-RU")) {
	///			// ��� ru-RU
	///			try {
	///				throw new LocalizableException();
	///			} catch (LocalizableException e) {
	///				// � ������� ��������� �������� �� ������� �����
	///				Console.WriteLine(e.Message);
	///				throw;
	///			}
	///		}
	///		// ��� ����� ������������� ��������
	///	} catch (LocalizableException e) {
	///		// ��������� ������ ��������� - �� ���������� �����
	///		Console.WriteLine(e.Message);
	///	}
	/// </code></example>
	public class UICulturePublisher : IDisposable {
		CultureInfo previous;

		/// <summary>������������������� ����� <see cref="UICulturePublisher"/>.</summary>
		/// <param name="cultureName">��� ����� UI ��������.</param>
		/// <remarks>����������� ������������� ������������� �������� �
		/// <see cref="System.Threading.Thread.CurrentUICulture"/> � � ������ <see cref="Dispose"/>
		/// ���������� ���������� �������� ��������, ����� ����� ������� ��������� ����� �������� � ��������������
		/// ��������� ����� using.</remarks>
		/// <exception cref="ArgumentNullException">���� <c>cultureName</c> ����� null (Nothing � Visual Basic).</exception>
		public UICulturePublisher(string cultureName):this(new CultureInfo(cultureName)) { }

		/// <summary>������������������� ����� <see cref="UICulturePublisher"/>.</summary>
		/// <param name="c">����� UI ��������.</param>
		/// <remarks>����������� ������������� ������������� �������� �
		/// <see cref="System.Threading.Thread.CurrentUICulture"/> � � ������ <see cref="Dispose"/>
		/// ���������� ���������� �������� ��������, ����� ����� ������� ��������� ����� �������� � ��������������
		/// ��������� ����� using.</remarks>
		/// <exception cref="ArgumentNullException">���� <c>c</c> ����� null (Nothing � Visual Basic).</exception>
		public UICulturePublisher(CultureInfo c) {
			if (c == null) throw new ArgumentNullException("c");
			previous = Thread.CurrentThread.CurrentUICulture;
			Thread.CurrentThread.CurrentUICulture = c;
		}

		/// <summary>������������ ���������� ��������.</summary>
		/// <remarks>����������� ������������� ������������� �������� �
		/// <see cref="System.Threading.Thread.CurrentUICulture"/> � � ������ <see cref="Dispose"/>
		/// ���������� ���������� �������� ��������, ����� ����� ������� ��������� ����� �������� � ��������������
		/// ��������� ����� using.</remarks>
		public void Dispose() {
			Thread.CurrentThread.CurrentUICulture = previous;
		}
	}

	/// <summary>���������� ��������.</summary>
	/// <remarks>�������� �� ������ ������� �������� ������ � ������������ �������������� ������,
	/// ���������� �����������. <see cref="CulturePublisher"/> ��������� ��� ������. �������� ������ ���
	/// <see cref="UICulturePublisher"/>.
	/// </remarks>
	public class CulturePublisher : IDisposable {
		CultureInfo previous;

		/// <summary>������������������� ����� <see cref="CulturePublisher"/>.</summary>
		/// <param name="cultureName">��� ����� ��������.</param>
		/// <remarks>����������� ������������� ������������� �������� �
		/// <see cref="System.Threading.Thread.CurrentCulture"/> � � ������ <see cref="Dispose"/>
		/// ���������� ���������� �������� ��������, ����� ����� ������� ��������� ����� �������� � ��������������
		/// ��������� ����� using.</remarks>
		/// <exception cref="ArgumentNullException">���� <c>cultureName</c> ����� null (Nothing � Visual Basic).</exception>
		public CulturePublisher(string cultureName):this(new CultureInfo(cultureName)) { }

		/// <summary>������������������� ����� <see cref="CulturePublisher"/>.</summary>
		/// <param name="c">����� ��������.</param>
		/// <remarks>����������� ������������� ������������� �������� �
		/// <see cref="System.Threading.Thread.CurrentCulture"/> � � ������ <see cref="Dispose"/>
		/// ���������� ���������� �������� ��������, ����� ����� ������� ��������� ����� �������� � ��������������
		/// ��������� ����� using.</remarks>
		/// <exception cref="ArgumentNullException">���� <c>c</c> ����� null (Nothing � Visual Basic).</exception>
		public CulturePublisher(CultureInfo c) {
			if (c == null) throw new ArgumentNullException("c");
			previous = Thread.CurrentThread.CurrentCulture;
			Thread.CurrentThread.CurrentCulture = c;
		}

		/// <summary>������������ ���������� ��������.</summary>
		/// <remarks>����������� ������������� ������������� �������� �
		/// <see cref="System.Threading.Thread.CurrentUICulture"/> � � ������ <see cref="Dispose"/>
		/// ���������� ���������� �������� ��������, ����� ����� ������� ��������� ����� �������� � ��������������
		/// ��������� ����� using.</remarks>
		public void Dispose() {
			Thread.CurrentThread.CurrentCulture = previous;
		}
	}
	
}
