// $Id: Log.cs 204 2006-04-12 10:33:03Z pilya $

#define	TRACE

using System;
using System.Diagnostics;

namespace Front.Diagnostics {

	// TODO DF0013: �� ������ � �������� ����� ������. ����� ���������� �� Log4Net
	/// <summary>Provides a set of methods and properties that help you trace the execution of your code.</summary>
	/// <threadsafety static="true" instance="true"/>
	public class Log {
		static TraceSwitch  defaultTraceSwitch = null;
		static Log          defaultLog = null;
		TraceSwitch         traceSwitch;
		public bool			recurseException = true;
		public bool			ShowStack = false;

		static Log() {
			defaultTraceSwitch = new TraceSwitch("Default", RM.GetString("DefSwitchDesc"));
			defaultLog = new Log(defaultTraceSwitch);
		}

		/// <summary>�������� ���, ������� ������� �� <see cref="TraceSwitch"/> "Default".</summary>
		/// <value>���, ������� ������� �� <see cref="TraceSwitch"/> "Default".</value>
		public static Log Default {
			get { return defaultLog; }
		}

		/// <summary>�������������� ����� <see cref="Log"/>, ������� ��������� <see cref="TraceSwitch"/> � ������
		/// <c>displayName</c></summary>
		/// <param name="displayName">��� <see cref="TraceSwitch"/>, �� ������� ����� ������� ���� <see cref="Log"/>.</param>
		public Log(string displayName):this(displayName, displayName) {}

		/// <summary>�������������� ����� <see cref="Log"/>, ������� ��������� <see cref="TraceSwitch"/> � ������
		/// <c>displayName</c> � ��������� ��������� ���������.</summary>
		/// <param name="displayName">��� <see cref="TraceSwitch"/>, �� ������� ����� ������� ���� <see cref="Log"/>.</param>
		/// <param name="description">�������� ���������, ������� ������� ���� <see cref="Log"/>.</param>
		public Log(string displayName, string description) : this(new TraceSwitch(displayName, description, defaultTraceSwitch.Level.ToString())) {
		}

		/// <summary>�������������� ����� <see cref="Log"/>, ������� ������� �� ���������� <see cref="TraceSwitch"/>.</summary>
		/// <param name="sw"><see cref="TraceSwitch"/>, �� ������� ����� ������� ���� <see cref="Log"/>.</param>
		public Log(TraceSwitch sw) {
			traceSwitch = sw;
		}

		/// <summary>������� � ��� ��������� �� ������.</summary>
		/// <param name="value">������, ��������� ������������� �������� ����� �������� � ���.</param>
		/// <remarks>��������� ����� �������� � ��� ������ � ��� ������, ���� <see cref="TraceLevel"/> �����
		/// ����� <c>TraceLevel.Error</c>, <c>TraceLevel.Warning</c>, <c>TraceLevel.Info</c> ��� <c>TraceLevel.Verbose</c>.
		/// </remarks>
		/// <seealso cref="Warn"/>
		/// <seealso cref="Info"/>
		/// <seealso cref="Verb"/>
		public void Fail(object value) {
			WriteLineIf(traceSwitch.TraceError, value);
		}

		/// <summary>������� � ��� ��������� �� ������.</summary>
		/// <param name="message">���������, ������� ����� �������� � ���.</param>
		/// <remarks>��������� ����� �������� � ��� ������ � ��� ������, ���� <see cref="TraceLevel"/> �����
		/// ����� <c>TraceLevel.Error</c>, <c>TraceLevel.Warning</c>, <c>TraceLevel.Info</c> ��� <c>TraceLevel.Verbose</c>.
		/// </remarks>
		/// <seealso cref="Warn"/>
		/// <seealso cref="Info"/>
		/// <seealso cref="Verb"/>
		public void Fail(string message) {
			WriteLineIf(traceSwitch.TraceError, "Error: " + message);
		}

		/// <summary>������� � ��� ��������� �� ������.</summary>
		/// <param name="e"><see cref="Exception"/>, ������� ����� ������� � ���.</param>
		/// <remarks>��������� ����� �������� � ��� ������ � ��� ������, ���� <see cref="TraceLevel"/> �����
		/// ����� <c>TraceLevel.Error</c>, <c>TraceLevel.Warning</c>, <c>TraceLevel.Info</c> ��� <c>TraceLevel.Verbose</c>.
		/// <para>� ����������� �� ���������� <see cref="RecurseException"/> � ��� ��������� ������ ���� ��������� ��
		/// ������ ��� ����� �������� � �������� � <see cref="Exception.InnerException"/>.</para>
		/// </remarks>
		/// <seealso cref="Warn"/>
		/// <seealso cref="Info"/>
		/// <seealso cref="Verb"/>
		public void Fail(Exception e) {
			WriteLineIf(traceSwitch.TraceError, e);
		}

		/// <summary>������� � ��� ��������� �� ������.</summary>
		/// <param name="format">������������� ������ ��� ���������, ������� ����� �������� � ���.</param>
		/// <param name="p">���������, ������� ����� ������������ ��� �������������� ���������.</param>
		/// <remarks>��������� ����� �������� � ��� ������ � ��� ������, ���� <see cref="TraceLevel"/> �����
		/// ����� <c>TraceLevel.Error</c>, <c>TraceLevel.Warning</c>, <c>TraceLevel.Info</c> ��� <c>TraceLevel.Verbose</c>.
		/// </remarks>
		/// <seealso cref="Warn"/>
		/// <seealso cref="Info"/>
		/// <seealso cref="Verb"/>
		public void Fail(string format, params object[] p) {
			WriteLineIf(traceSwitch.TraceError, "Error: " + format, p);
		}

		/// <summary>������� � ��� ��������� � ��������������.</summary>
		/// <param name="value">������, ��������� ������������� �������� ����� �������� � ���.</param>
		/// <remarks>��������� ����� �������� � ��� ������ � ��� ������, ���� <see cref="TraceLevel"/> �����
		/// ����� <c>TraceLevel.Warning</c>, <c>TraceLevel.Info</c> ��� <c>TraceLevel.Verbose</c>.
		/// </remarks>
		/// <seealso cref="Fail"/>
		/// <seealso cref="Info"/>
		/// <seealso cref="Verb"/>
		public void Warn(object value) {
			WriteLineIf(traceSwitch.TraceWarning, value);
		}

		/// <summary>������� � ��� ��������� � ��������������.</summary>
		/// <param name="message">���������, ������� ����� �������� � ���.</param>
		/// <remarks>��������� ����� �������� � ��� ������ � ��� ������, ���� <see cref="TraceLevel"/> �����
		/// ����� <c>TraceLevel.Warning</c>, <c>TraceLevel.Info</c> ��� <c>TraceLevel.Verbose</c>.
		/// </remarks>
		/// <seealso cref="Fail"/>
		/// <seealso cref="Info"/>
		/// <seealso cref="Verb"/>
		public void Warn(string message) {
			WriteLineIf(traceSwitch.TraceWarning, "Warning: " + message);
		}

		/// <summary>������� � ��� ��������� � ��������������.</summary>
		/// <param name="format">������������� ������ ��� ���������, ������� ����� �������� � ���.</param>
		/// <param name="p">���������, ������� ����� ������������ ��� �������������� ���������.</param>
		/// <remarks>��������� ����� �������� � ��� ������ � ��� ������, ���� <see cref="TraceLevel"/> �����
		/// ����� <c>TraceLevel.Warning</c>, <c>TraceLevel.Info</c> ��� <c>TraceLevel.Verbose</c>.
		/// </remarks>
		/// <seealso cref="Fail"/>
		/// <seealso cref="Info"/>
		/// <seealso cref="Verb"/>
		public void Warn(string format, params object[] p) {
			WriteLineIf(traceSwitch.TraceWarning, "Warning: " + format, p);
		}

		/// <summary>������� � ��� �������������� ���������.</summary>
		/// <param name="value">������, ��������� ������������� �������� ����� �������� � ���.</param>
		/// <remarks>��������� ����� �������� � ��� ������ � ��� ������, ���� <see cref="TraceLevel"/> �����
		/// ����� <c>TraceLevel.Info</c> ��� <c>TraceLevel.Verbose</c>.
		/// </remarks>
		/// <seealso cref="Fail"/>
		/// <seealso cref="Warn"/>
		/// <seealso cref="Verb"/>
		public void Info(object value) {
			WriteLineIf(traceSwitch.TraceInfo, value);
		}

		/// <summary>������� � ��� �������������� ���������.</summary>
		/// <param name="message">���������, ������� ����� �������� � ���.</param>
		/// <remarks>��������� ����� �������� � ��� ������ � ��� ������, ���� <see cref="TraceLevel"/> �����
		/// ����� <c>TraceLevel.Info</c> ��� <c>TraceLevel.Verbose</c>.
		/// </remarks>
		/// <seealso cref="Fail"/>
		/// <seealso cref="Warn"/>
		/// <seealso cref="Verb"/>
		public void Info(string message) {
			WriteLineIf(traceSwitch.TraceInfo, message);
		}

		/// <summary>������� � ��� �������������� ���������.</summary>
		/// <param name="format">������������� ������ ��� ���������, ������� ����� �������� � ���.</param>
		/// <param name="p">���������, ������� ����� ������������ ��� �������������� ���������.</param>
		/// <remarks>��������� ����� �������� � ��� ������ � ��� ������, ���� <see cref="TraceLevel"/> �����
		/// ����� <c>TraceLevel.Info</c> ��� <c>TraceLevel.Verbose</c>.
		/// </remarks>
		/// <seealso cref="Fail"/>
		/// <seealso cref="Warn"/>
		/// <seealso cref="Verb"/>
		public void Info(string format, params object[] p) {
			WriteLineIf(traceSwitch.TraceInfo, format, p);
		}

		/// <summary>������� � ��� ���������, ���������� ��������� ����������.</summary>
		/// <param name="value">������, ��������� ������������� �������� ����� �������� � ���.</param>
		/// <remarks>��������� ����� �������� � ��� ������ � ��� ������, ���� <see cref="TraceLevel"/> �����
		/// ����� <c>TraceLevel.Verbose</c>. ������, ���� ������� ����������� ����� ����������� ������ ���
		/// ������� ����������.</remarks>
		/// <seealso cref="Fail"/>
		/// <seealso cref="Warn"/>
		/// <seealso cref="Info"/>
		public void Verb(object value) {
			WriteLineIf(traceSwitch.TraceVerbose, value);
		}

		/// <summary>������� � ��� ���������, ���������� ��������� ����������.</summary>
		/// <param name="message">���������, ������� ����� �������� � ���.</param>
		/// <remarks>��������� ����� �������� � ��� ������ � ��� ������, ���� <see cref="TraceLevel"/> �����
		/// ����� <c>TraceLevel.Verbose</c>. ������, ���� ������� ����������� ����� ����������� ������ ���
		/// ������� ����������.</remarks>
		/// <seealso cref="Fail"/>
		/// <seealso cref="Warn"/>
		/// <seealso cref="Info"/>
		public void Verb(string message) {
			WriteLineIf(traceSwitch.TraceVerbose, message);
		}

		/// <summary>������� � ��� ���������, ���������� ��������� ����������.</summary>
		/// <param name="format">������������� ������ ��� ���������, ������� ����� �������� � ���.</param>
		/// <param name="p">���������, ������� ����� ������������ ��� �������������� ���������.</param>
		/// <remarks>��������� ����� �������� � ��� ������ � ��� ������, ���� <see cref="TraceLevel"/> �����
		/// ����� <c>TraceLevel.Verbose</c>. ������, ���� ������� ����������� ����� ����������� ������ ���
		/// ������� ����������.</remarks>
		/// <seealso cref="Fail"/>
		/// <seealso cref="Warn"/>
		/// <seealso cref="Info"/>
		public void Verb(string format, params object[] p) {
			WriteLineIf(traceSwitch.TraceVerbose, format, p);
		}

		/// <summary>������� � ��� ��������� � ������ ������.</summary>
		/// <param name="format">���������������� �������� ������.</param>
		/// <param name="p">���������, ������� ����� ������������ ��� �������������� ���������.</param>
		/// <remarks><para>��������� ����� �������� � ��� ������ � ��� ������, ���� <see cref="TraceLevel"/> �����
		/// ����� <c>TraceLevel.Verbose</c>. ������, ���� ������� ����������� ����� ����������� ������ ���
		/// ������� ����������.</para>
		/// <para>��������� ����������� �� <c>TraceSwitch.DisplayName</c> � ��������� ������. ��������������,
		/// ��� <c>TraceSwitch.DisplayName</c> ������������� ����� ������ (���� ����� ��� � ����).</para></remarks>
		/// <seealso cref="Fail"/>
		/// <seealso cref="Warn"/>
		/// <seealso cref="Info"/>
		public void Method(string format, params object[] p) {
			// TODO: �������� �������� ������ � ������ �� StackTrace
			Verb(String.Format("[{0}].{1}", traceSwitch.DisplayName, format), p);
		}

		/// <summary>������� � ��� ��������� ���������.</summary>
		/// <param name="value">������, ��������� ������������� �������� ����� �������� � ���.</param>
		/// <remarks>��������� ����� �������� � ����� ������. ��� ��������� ������ ��������� �����������
		/// ������ <see cref="Fail"/>, <see cref="Warn"/>, <see cref="Info"/> � <see cref="Verb"/>.</remarks>
		/// <seealso cref="WriteIf"/>
		/// <seealso cref="WriteLine"/>
		/// <seealso cref="WriteLineIf"/>
		public virtual void Write(object value) {
			WriteIf(traceSwitch.TraceVerbose, value);
		}

		/// <summary>������� � ��� ��������� ���������.</summary>
		/// <param name="message">���������, ������� ����� �������� � ���.</param>
		/// <remarks>��������� ����� �������� � ����� ������. ��� ��������� ������ ��������� �����������
		/// ������ <see cref="Fail"/>, <see cref="Warn"/>, <see cref="Info"/> � <see cref="Verb"/>.</remarks>
		/// <seealso cref="WriteIf"/>
		/// <seealso cref="WriteLine"/>
		/// <seealso cref="WriteLineIf"/>
		public virtual void Write(string message) {
			WriteIf(traceSwitch.TraceVerbose, message);
		}

		/// <summary>������� � ��� ��������� ���������.</summary>
		/// <param name="format">������������� ������ ��� ���������, ������� ����� �������� � ���.</param>
		/// <param name="p">���������, ������� ����� ������������ ��� �������������� ���������.</param>
		/// <remarks>��������� ����� �������� � ����� ������. ��� ��������� ������ ��������� �����������
		/// ������ <see cref="Fail"/>, <see cref="Warn"/>, <see cref="Info"/> � <see cref="Verb"/>.</remarks>
		/// <seealso cref="WriteIf"/>
		/// <seealso cref="WriteLine"/>
		/// <seealso cref="WriteLineIf"/>
		public virtual void Write(string format, params object[] p) {
			WriteIf(traceSwitch.TraceVerbose, format, p);
		}

		/// <summary>������� � ��� ��������� ��������� � ����������� �������� �������.</summary>
		/// <param name="value">������, ��������� ������������� �������� ����� �������� � ���.</param>
		/// <remarks>��������� ����� �������� � ����� ������. ��� ��������� ������ ��������� �����������
		/// ������ <see cref="Fail"/>, <see cref="Warn"/>, <see cref="Info"/> � <see cref="Verb"/>.</remarks>
		/// <seealso cref="Write"/>
		/// <seealso cref="WriteIf"/>
		/// <seealso cref="WriteLineIf"/>
		public virtual void WriteLine(object value) {
			WriteLineIf(traceSwitch.TraceVerbose, value);
		}

		/// <summary>������� � ��� ��������� ��������� � ����������� �������� �������.</summary>
		/// <param name="e"><see cref="Exception"/>, ������� ����� ������� � ���.</param>
		/// <remarks>��������� ����� �������� � ����� ������. ��� ��������� ������ ��������� �����������
		/// ������ <see cref="Fail"/>, <see cref="Warn"/>, <see cref="Info"/> � <see cref="Verb"/>.</remarks>
		/// <seealso cref="Write"/>
		/// <seealso cref="WriteIf"/>
		/// <seealso cref="WriteLineIf"/>
		public virtual void WriteLine(Exception e) {
			WriteLineIf(traceSwitch.TraceVerbose, e);
		}

		/// <summary>������� � ��� ��������� ��������� � ����������� �������� �������.</summary>
		/// <param name="format">������������� ������ ��� ���������, ������� ����� �������� � ���.</param>
		/// <param name="p">���������, ������� ����� ������������ ��� �������������� ���������.</param>
		/// <remarks>��������� ����� �������� � ����� ������. ��� ��������� ������ ��������� �����������
		/// ������ <see cref="Fail"/>, <see cref="Warn"/>, <see cref="Info"/> � <see cref="Verb"/>.</remarks>
		/// <seealso cref="Write"/>
		/// <seealso cref="WriteIf"/>
		/// <seealso cref="WriteLineIf"/>
		public virtual void WriteLine(string format, params object[] p) {
			WriteLineIf(traceSwitch.TraceVerbose, format, p);
		}

		/// <summary>������� � ��� ��������� ��������� � ����������� �������� �������.</summary>
		/// <param name="message">���������, ������� ����� �������� � ���.</param>
		/// <remarks>��������� ����� �������� � ����� ������. ��� ��������� ������ ��������� �����������
		/// ������ <see cref="Fail"/>, <see cref="Warn"/>, <see cref="Info"/> � <see cref="Verb"/>.</remarks>
		/// <seealso cref="Write"/>
		/// <seealso cref="WriteIf"/>
		/// <seealso cref="WriteLineIf"/>
		public virtual void WriteLine(string message) {
			WriteLineIf(traceSwitch.TraceVerbose, message);
		}

		# region �������� ������
		/// <summary>������� � ��� ��������� ���������.</summary>
		/// <param name="condition">�������, ��� ������� ��������� ����� �������� � ���.</param>
		/// <param name="value">������, ��������� ������������� �������� ����� �������� � ���.</param>
		/// <remarks>��������� ����� �������� � ������ � ��� ������, ���� <c>condition = true.</c>
		/// ����������� ����� ������� ������������� ��� ��������� ������ ��������� 
		/// ������� <see cref="Fail"/>, <see cref="Warn"/>, <see cref="Info"/> � <see cref="Verb"/>.</remarks>
		/// <seealso cref="Write"/>
		/// <seealso cref="WriteLine"/>
		/// <seealso cref="WriteLineIf"/>
		public void WriteIf(bool condition, object value) {
			string message = (value == null) ? "<null>" : value.ToString();
			WriteIf(condition, message);
		}

		/// <summary>������� � ��� ��������� ���������.</summary>
		/// <param name="condition">�������, ��� ������� ��������� ����� �������� � ���.</param>
		/// <param name="format">������������� ������ ��� ���������, ������� ����� �������� � ���.</param>
		/// <param name="p">���������, ������� ����� ������������ ��� �������������� ���������.</param>
		/// <remarks>��������� ����� �������� � ������ � ��� ������, ���� <c>condition = true.</c>
		/// ����������� ����� ������� ������������� ��� ��������� ������ ��������� 
		/// ������� <see cref="Fail"/>, <see cref="Warn"/>, <see cref="Info"/> � <see cref="Verb"/>.</remarks>
		/// <seealso cref="Write"/>
		/// <seealso cref="WriteLine"/>
		/// <seealso cref="WriteLineIf"/>
		public void WriteIf(bool condition, string format, params object[] p) {
			string message = (p != null) ? String.Format(format, p) : format;
			WriteIf(condition, message);
		}

		/// <summary>������� � ��� ��������� ��������� � ����������� �������� �������.</summary>
		/// <param name="condition">�������, ��� ������� ��������� ����� �������� � ���.</param>
		/// <param name="value">������, ��������� ������������� �������� ����� �������� � ���.</param>
		/// <remarks>��������� ����� �������� � ������ � ��� ������, ���� <c>condition = true.</c>
		/// ����������� ����� ������� ������������� ��� ��������� ������ ��������� 
		/// ������� <see cref="Fail"/>, <see cref="Warn"/>, <see cref="Info"/> � <see cref="Verb"/>.</remarks>
		/// <seealso cref="Write"/>
		/// <seealso cref="WriteIf"/>
		/// <seealso cref="WriteLine"/>
		public void WriteLineIf(bool condition, object value) {
			string message = (value == null) ? "<null>" : value.ToString();
			WriteLineIf(condition, message);
		}

		/// <summary>������� � ��� ��������� ��������� � ����������� �������� �������.</summary>
		/// <param name="condition">�������, ��� ������� ��������� ����� �������� � ���.</param>
		/// <param name="e"><see cref="Exception"/>, ������� ����� ������� � ���.</param>
		/// <remarks>��������� ����� �������� � ������ � ��� ������, ���� <c>condition = true.</c>
		/// ����������� ����� ������� ������������� ��� ��������� ������ ��������� 
		/// ������� <see cref="Fail"/>, <see cref="Warn"/>, <see cref="Info"/> � <see cref="Verb"/>.</remarks>
		/// <seealso cref="Write"/>
		/// <seealso cref="WriteIf"/>
		/// <seealso cref="WriteLine"/>
		public void WriteLineIf(bool condition, Exception e) {
			string message = (e != null) ? ExceptionToString(e) : "<null>";
			WriteLineIf(condition, message);
		}

		/// <summary>������� � ��� ��������� ��������� � ����������� �������� �������.</summary>
		/// <param name="condition">�������, ��� ������� ��������� ����� �������� � ���.</param>
		/// <param name="format">������������� ������ ��� ���������, ������� ����� �������� � ���.</param>
		/// <param name="p">���������, ������� ����� ������������ ��� �������������� ���������.</param>
		/// <remarks>��������� ����� �������� � ������ � ��� ������, ���� <c>condition = true.</c>
		/// ����������� ����� ������� ������������� ��� ��������� ������ ��������� 
		/// ������� <see cref="Fail"/>, <see cref="Warn"/>, <see cref="Info"/> � <see cref="Verb"/>.</remarks>
		/// <seealso cref="Write"/>
		/// <seealso cref="WriteIf"/>
		/// <seealso cref="WriteLine"/>
		public void WriteLineIf(bool condition, string format, params object[] p) {
			// XXX ��� ������� Exception'��� ��� ��������������!
			string message = (p != null) ? String.Format(format, p) : format;
			WriteLineIf(condition, message);
		}

		/// <summary>������� � ��� ��������� ���������.</summary>
		/// <param name="condition">�������, ��� ������� ��������� ����� �������� � ���.</param>
		/// <param name="message">���������, ������� ����� �������� � ���.</param>
		/// <remarks>��������� ����� �������� � ������ � ��� ������, ���� <c>condition = true.</c>
		/// ����������� ����� ������� ������������� ��� ��������� ������ ��������� 
		/// ������� <see cref="Fail"/>, <see cref="Warn"/>, <see cref="Info"/> � <see cref="Verb"/>.</remarks>
		/// <seealso cref="Write"/>
		/// <seealso cref="WriteLine"/>
		/// <seealso cref="WriteLineIf"/>
		public void WriteIf(bool condition, string message) {
			if (condition) Trace.Write(message);
		}

		/// <summary>������� � ��� ��������� ��������� � ����������� �������� �������.</summary>
		/// <param name="condition">�������, ��� ������� ��������� ����� �������� � ���.</param>
		/// <param name="message">���������, ������� ����� �������� � ���.</param>
		/// <remarks>��������� ����� �������� � ������ � ��� ������, ���� <c>condition = true.</c>
		/// ����������� ����� ������� ������������� ��� ��������� ������ ��������� 
		/// ������� <see cref="Fail"/>, <see cref="Warn"/>, <see cref="Info"/> � <see cref="Verb"/>.</remarks>
		/// <seealso cref="Write"/>
		/// <seealso cref="WriteIf"/>
		/// <seealso cref="WriteLine"/>
		public void WriteLineIf(bool condition, string message) {
			if (condition) 
				Trace.WriteLine(message);
		}
		#endregion

		/// <summary>�������� ��� ���������� ������� ����������� �����.</summary>
		/// <value>�������� <see cref="TraceLevel"/>, ����������� ������� ������� �����.</value>
		/// <remarks>��� �������� ���������� <see cref="Front.Diagnostics.Log"/>, ������� ����� �������������
		/// �������� <c>TraceLevel</c> ������� <see cref="TraceSwitch"/>, ��
		/// ������� ������� <see cref="Front.Diagnostics.Log"/>.</remarks>
		public TraceLevel Level {
			get { return traceSwitch.Level; }
			set { traceSwitch.Level = value; }
		}

		/// <summary>�������� ��� ���������� �������� �����������, ����� �� ������������ ����������� �����
		/// ��������� ���������� ��� ������ � ��� <see cref="Exception"/>.</summary>
		/// <value>true, ���� ��� ������ ����� ����� ������������ ����� ��������� ����������; ����� false</value>
		public bool RecurseException {
			get { return recurseException; }
			set { recurseException = value; }
		}

		/// <summary>�������� ��������� ������������� ������� <see cref="Exception"/>.</summary>
		/// <value>��������� ������������� ������� <see cref="Exception"/>.</value>
		/// <remarks>���� �������� <see cref="RecurseException"/> ����� �������� <c>false</c>, �� ����
		/// ����� ������ ��������� ������ <see cref="Exception.ToString"/> � ������� <c>e</c>.
		/// �����, ��������� ����� ��������� ��� ��������� ����������.</remarks>
		/// <exception cref="ArgumentNullException">���� <c>e</c> ����� <c>null</c>.</exception>
		protected virtual string ExceptionToString(Exception e) 
		{
			if (e == null) 
				throw new ArgumentNullException("e");
			if (recurseException) {
				System.Text.StringBuilder sb = new System.Text.StringBuilder(400);
				while (e != null) {
					sb.AppendFormat("{0}", (ShowStack) ? e.ToString(): e.Message );
					
					e = e.InnerException;
					if (e != null)
						sb.AppendFormat("\n{0}\n", RM.GetString("LogNestedException"));
				}
				return sb.ToString();
			} else
				return e.ToString();
		}

		public virtual void Indent() {
			Trace.Indent();
		}

		public virtual void Unindent() {
			Trace.Unindent();
		}
	}
}

