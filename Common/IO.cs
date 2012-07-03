// $Id: IO.cs 657 2006-05-30 13:31:52Z pilya $

using System;
using System.Collections;
using System.Threading;
using System.Diagnostics;
using System.IO;

namespace Front.IO {
	// TODO DF0023: ������� ������������� � ���������� ������������� ������. ��� ����� ��������� � FS

	/// <summary>������������� ��� �������, ������� ����� ���� ������������ ��� �������� �����-������.</summary>
	/// <remarks>������������� ������������ <see cref="BufferPool.Allocate"/> ������ ������ ���������
	/// ������ ����� �������� ����� ������� ������. ��� ���� ����������� ������������ ���� � ��� ��
	/// ����� ��������.</remarks>
	public class BufferPool : MarshalByRefObject, IDisposable {
		/// <summary>���������� ���������� ������ ��� ������ <see cref="BufferPool"/>.</summary>
		/// <remarks>������, ���������� ����� ������ �� ��������� �������� �����������, �
		/// ���������� �������� �� � <srr cref="BufferPool"/> ������� ������ <see cref="BufferPool.Allocate"/>,
		/// �� ����� �������� �� ���������.</remarks>
		public class Buffer : IDisposable {
			BufferPool		pool;
			byte[]			buffer;
			int				usedFlag;
			bool			mustClean;

			/// <summary>������� ����� <see cref="Buffer"/>.</summary>
			/// <param name="pool"><see cref="BufferPool"/>, ������� ����� ��������� �������.</param>
			/// <param name="size">������ ������ ������ � ������.</param>
			/// <remarks><paramref name="pool"/> ����� ��������� �������� <b>null</b>, ��� �������� ���
			/// ����� ������� �� ����� ��������� <see cref="BufferPool"/>.</remarks>
			public Buffer(BufferPool pool, int size) {
				this.pool = pool;
				buffer = new byte[size];
				mustClean = false;
			}

			/// <summary>��������� ����� � ��������� "������������".</summary>
			/// <remarks>������� ������ � ��������� "������������" ������������ ���������
			/// ��� ������� <see cref="BufferPool.Allocate"/> ������� <see cref="BufferPool"/>,
			/// ������� ��������� ������ �������.
			/// <para>� ������ ����� <see cref="Buffer"/> ������ ���� � �� �� ��������� <see cref="BufferPool"/>
			/// ������ ������� <see cref="Allocate"/> � <see cref="Release"/> �� ����� �������� �������.</para>
			/// </remarks>
			/// <returns>true, ���� ����� �� ����� ��� � ��������� "�� ������������" � ������� ��������� ��� �
			/// ��������� "������������"; ����� false</returns>
			public bool Allocate() {
				return Allocate(false);
			}

			/// <summary>��������� ����� � ��������� "������������".</summary>
			/// <param name="cleanOnRelease">��������� ������� �� ������� ����� ����� ��������� ��� � ���.</param>
			/// <remarks>������� ������ � ��������� "������������" ������������ ���������
			/// ��� ������� <see cref="BufferPool.Allocate"/> ������� <see cref="BufferPool"/>,
			/// ������� ��������� ������ �������.
			/// <para>� ������ ����� <see cref="Buffer"/> ������ ���� � �� �� ��������� <see cref="BufferPool"/>
			/// ������ ������� <see cref="Allocate"/> � <see cref="Release"/> �� ����� �������� �������.</para>
			/// </remarks>
			/// <returns>true, ���� ����� �� ����� ��� � ��������� "�� ������������" � ������� ��������� ��� �
			/// ��������� "������������"; ����� false</returns>
			public bool Allocate(bool cleanOnRelease) {
				int flag = Interlocked.CompareExchange(ref usedFlag, 1, 0);
				if (flag == 0) {
					// ���� ���������� "������������" �������� ������� - ������
					if (mustClean)
						System.Array.Clear(buffer, 0, buffer.Length);
					// ���������� ��� ����� ���� ������������
					mustClean = cleanOnRelease;
					return true;
				} else
					return false;	
			}

			/// <summary>��������� ����� � ��������� "�� ������������".</summary>
			/// <remarks>����� ������ ����� ������ ������ ����� ����� ���� ������� �������
			/// <see cref="BufferPool.Allocate"/> ������� <see cref="BufferPool"/>,
			/// ������� ��������� ������ �������.
			/// <para>� ������ ����� <see cref="Buffer"/> ������ ���� � �� �� ��������� <see cref="BufferPool"/>
			/// ������ ������� <see cref="Allocate"/> � <see cref="Release"/> �� ����� �������� �������.</para>
			/// </remarks>
			public void Release() {
				Interlocked.Exchange(ref usedFlag, 0);
				if (pool != null) pool.mrEvent.Set();
			}

			/// <summary>���������� ����� (������ ��� �������� �����-������).</summary>
			/// <value>������ ������, ������� ������������ <see cref="Buffer"/> � �������� ������.</value>
			public byte[] Array { get { return buffer; } }

			/// <summary>�������� ������ ������ � ������.</summary>
			/// <value>������ ������ � ������.</value>
			public int Size { get { return buffer.Length; } }

			/// <summary>������ ����� �� ������ ���� �����.</summary>
			/// <value>true, ���� ���� ����� ������ ������������ � false, ���� �� ��������.</value>
			public bool InUse { get { return (usedFlag != 0); } }

			void IDisposable.Dispose() {
				Release();
			}

			/// <summary>������������� <see cref="Buffer"/> � <c>byte[]</c>.</summary>
			/// <remarks>��-����, ��� �������������� ����������� ��������� � �������� <see cref="Array"/> � ������
			/// ����� ������. ��� ������������� ��� ��������� ������������� <see cref="BufferPool"/> � ����� C#.</remarks>
			public static implicit operator byte[](Buffer b) {
				return b.Array;
			}
		}
		
		int					granularity;
		int					maxBufferCount;
		ArrayList			buffers = new ArrayList();
		ReaderWriterLock	rwLock = new ReaderWriterLock();
		ManualResetEvent	mrEvent = new ManualResetEvent(false);

		/// <summary>Initializes a new instance of the <see cref="BufferPool"/> class.</summary>
		/// <remarks>������� <see cref="BufferPool"/> c �������������� ����������� ������� � �������������� ������ 1024.</remarks>
		public BufferPool():this(1024) {}

		/// <summary>Initializes a new instance of the <see cref="BufferPool"/> class.</summary>
		/// <remarks>������� <see cref="BufferPool"/> c �������������� ����������� ������� � �������� ��������������</remarks>
		/// <param name="granularity">������������� ����. �� �������� �������� �������������, ����������� ������
		/// ���� �������, ����������� <see cref="BufferPool"/>.</param>
		public BufferPool(int granularity):this(granularity, 0) {}

		/// <summary>Initializes a new instance of the <see cref="BufferPool"/> class.</summary>
		/// <remarks>������� <see cref="BufferPool"/> c ����������� �������,
		/// ������������ <paramref name="maxBufferCount"/>. ���� <paramref name="maxBufferCount"/> ���������
		/// �������� 0, �� ���������� ������� ������������.</remarks>
		/// <param name="granularity">������������� ����. �� �������� �������� �������������, ����������� ������
		/// ���� �������, ����������� <see cref="BufferPool"/>.</param>
		/// <param name="maxBufferCount">������������ ���������� �������, ������� ����� ������������ ������������
		/// � ������ <see cref="BufferPool"/>.</param>
		public BufferPool(int granularity, int maxBufferCount) {
			this.granularity	= granularity;
			this.maxBufferCount	= maxBufferCount;
		}

		/// <summary>���������� ������������� ������� � <see cref="IDisposable"/> �������.</summary>
		public void Dispose() {
			Dispose(true);
		}

		/// <summary>���������� ������������� ������� � <see cref="IDisposable"/> �������.</summary>
		protected virtual void Dispose(bool disposing) {
			if (disposing) {
				GC.SuppressFinalize(this);
				if (mrEvent != null) {
					mrEvent.Close();
					mrEvent = null;
				}
			}
		}

		/// <summary>�������� �����.</summary>
		/// <param name="reqSize">������ ������, ������� ���������� �������.</param>
		/// <remarks>���� ����� ������� �����, ������� �� ����� ������ ��� ������������.
		/// <para>���� ���������� ������ ���, �� ����� ����� ����� ����������� ������� ���
		/// �������������� �����.</para>
		/// <para>������� ���������, ��� ��� �������� ������ ������ ����� <see cref="Allocate"/> �����������
		/// ��� ������������ ����� ����� ����� ������ �� ������� ��� <paramref name="reqSize"/>. ��� ���� �����\
		/// ����� ���� ������.</para>
		/// </remarks>
		/// <seealso cref="CollectLostBuffers"/>
		/// <seealso cref="Granularity"/>
		public Buffer Allocate(int reqSize) {
			return Allocate(reqSize, false, Timeout.Infinite);
		}
		
		/// <summary>�������� �����.</summary>
		/// <param name="reqSize">������ ������, ������� ���������� �������.</param>
		/// <param name="cleanOnRelease">���������� ������� ����� ��� �������� ��� � ���.</param>
		/// <remarks>���� ���������� ������ ���, �� ����� ����� ����� ����������� ������� ���
		/// �������������� �����.<para>������� ���������, ��� ��� �������� ������ ������ ����� <see cref="Allocate"/> �����������
		/// ��� ������������ ����� ����� ����� ������ �� ������� ��� <paramref name="reqSize"/>. ��� ���� �����\
		/// ����� ���� ������.</para>
		/// </remarks>
		/// <seealso cref="CollectLostBuffers"/>
		/// <seealso cref="Granularity"/>
		public Buffer Allocate(int reqSize, bool cleanOnRelease) {
			return Allocate(reqSize, cleanOnRelease, Timeout.Infinite);
		}

		/// <summary>�������� �����.</summary>
		/// <param name="reqSize">������ ������, ������� ���������� �������.</param>
		/// <param name="timeOut">����� � �������������, ������� ����� ����� �����, � ������ ���� ������ ���.</param>
		/// <remarks>���� ����� ������� �����, ������� �� ����� ������ ��� ������������.
		/// <para>���� ���������� ������ ���, �� ����� ����� ����� ����������� ������� ��� <paramref name="timeOut"/>
		/// �����������.</para>
		/// <para>������� ���������, ��� ��� �������� ������ ������ ����� <see cref="Allocate"/> �����������
		/// ��� ������������ ����� ����� ����� ������ �� ������� ��� <paramref name="reqSize"/>. ��� ���� �����\
		/// ����� ���� ������.</para>
		/// </remarks>
		/// <seealso cref="CollectLostBuffers"/>
		/// <seealso cref="Granularity"/>
		public Buffer Allocate(int reqSize, int timeOut) {
			return Allocate(reqSize, false, timeOut);
		}

		/// <summary>�������� �����.</summary>
		/// <param name="reqSize">������ ������, ������� ���������� �������.</param>
		/// <param name="cleanOnRelease">���������� ������� ����� ��� �������� ��� � ���.</param>
		/// <param name="timeOut">����� � �������������, ������� ����� ����� �����, � ������ ���� ������ ���.</param>
		/// <remarks>���� ����� ������� �����, ������� ����� ���� ������ ��� ������������.
		/// <para>���� ���������� ������ ���, �� ����� ����� ����� ����������� ������� ��� <paramref name="timeOut"/>
		/// �����������.</para>
		/// <para>������� ���������, ��� ��� �������� ������ ������ ����� <see cref="Allocate"/> �����������
		/// ��� ������������ ����� ����� ����� ������ �� ������� ��� <paramref name="reqSize"/>. ��� ���� �����\
		/// ����� ���� ������.</para>
		/// </remarks>
		/// <seealso cref="CollectLostBuffers"/>
		/// <seealso cref="Granularity"/>
		public virtual Buffer Allocate(int reqSize, bool cleanOnRelease, int timeOut) {
			bool deadFound = false;
			if (mrEvent == null)
				throw new ObjectDisposedException(this.GetType().Name);
			Buffer result = null;
			bool wait = false;
			DateTime deadLine = (timeOut == Timeout.Infinite)
				? DateTime.MaxValue
				: DateTime.Now + TimeSpan.FromMilliseconds(timeOut);
			do {
				if (wait) {
					if (mrEvent.WaitOne(
							(int)(deadLine - DateTime.Now).TotalMilliseconds, false ))
						mrEvent.Reset();
					else
						throw new ApplicationException(RM.GetString("ErrTimeout"));
				}
				rwLock.AcquireReaderLock(
						(timeOut > 0)
							? (int)(deadLine - DateTime.Now).TotalMilliseconds
							: timeOut);
				if (!rwLock.IsReaderLockHeld)
					break;  // ApplicationException will be throwed
				try {
					foreach (WeakReference wr in buffers) {
						if (wr.IsAlive) {
							Buffer b = wr.Target as Buffer;
							if (b.Size >= reqSize && b.Allocate(cleanOnRelease)) {
								result = b;
								break;
							}
						} else
							deadFound = true;
					}

					if (result == null && (maxBufferCount == 0 || buffers.Count < maxBufferCount)) {
						rwLock.UpgradeToWriterLock(
								(timeOut > 0)
									? (int)(deadLine - DateTime.Now).TotalMilliseconds
									: timeOut);
						if (!rwLock.IsWriterLockHeld)
							break; // ApplicationException will be throwed
						result = NewAllocatedBuffer(reqSize, cleanOnRelease);
						buffers.Add(new WeakReference(result));
					}
				} finally {
					rwLock.ReleaseReaderLock();
				}
				wait = true;
			} while (result == null && DateTime.Now < deadLine);
			if (deadFound)
				ThreadPool.QueueUserWorkItem(new WaitCallback(CollectLostBuffers));
			if (result == null && timeOut != 0)
				throw new ApplicationException(RM.GetString("ErrTimeout"));
			return result;
		}

		/// <summary> �������� ����� ����� � ��������� ��� � ��������� "������������".</summary>
		/// <param name="size">������ ������ � ������</param>
		/// <param name="cleanOnRelease">true, ���� ����� ���� ������� ��� �������� � ���.</param>
		/// <remarks>���� ����� ����� �������� ����� �� ���� �������, ������� ��� �������� �������������.
		/// ������������� ���������� ������ ��������� ������ ������ �� �������� <see cref="Granularity"/>.</remarks>
		/// <seealso cref="Allocate"/>
		protected virtual Buffer NewAllocatedBuffer(int size, bool cleanOnRelease) {
			int mod = size % granularity;
			size = granularity * ((size / granularity) + ((size % granularity > 0) ? 1 : 0));
			Buffer r = new Buffer(this, size);
			r.Allocate(cleanOnRelease);
			return r;
		}

		// ��. CollectLostBuffers()
		void CollectLostBuffers(object state) {
			CollectLostBuffers();
		}

		/// <summary> �������� ����������� ����. </summary>
		/// <remarks> ��� ������ �� ������, ������� ����������� ��������� ������, �����
		/// ������� �� <see cref="BufferPool"/>.<para>������ ���������� �� ��������� ��������
		/// ���� ����� ��������.</para></remarks>
		/// <seealso cref="Allocate"/>
		public void CollectLostBuffers() {
			rwLock.AcquireWriterLock(Timeout.Infinite);
			if (rwLock.IsWriterLockHeld) try {
				int index = 0;
				while (index < buffers.Count) {
					WeakReference wr = buffers[index] as WeakReference;
					if (!wr.IsAlive)
						buffers.RemoveAt(index);
					else
						index++;
				}
			} finally {
				rwLock.ReleaseWriterLock();
			}
		}

		/// <summary> ������� ���������� ������� � ����. </summary>
		/// <value> ���������� ������� � ����. </value>
		/// <remarks> ������ ����� ���������� ���������� ������ �� ������, �������� ���������
		/// <see cref="BufferPool"/>. ��������� �� ���� ������� ��� ����� ���� ������� ���������
		/// ������. </remarks>
		/// <seealso cref="UsedMemory"/>
		public int ActiveBuffers {
			get {
				rwLock.AcquireReaderLock(Timeout.Infinite);
				int cnt = buffers.Count;
				rwLock.ReleaseReaderLock();
				return cnt;
			}
		}

		/// <summary> ������� ��������� ����� ������, ���������� ����� ��������� ��������. </summary>
		/// <value> ��������� ����� ������ ����������� <see cref="BufferPool"/>. </value>
		/// <remarks> ����� ��������� ������ �������� ������, �� ������� ��� �� ������� ��������� ������. </remarks>
		/// <seealso cref="ActiveBuffers"/>
		public int UsedMemory {
			get {
				int size = 0;
				if (mrEvent != null) {
					rwLock.AcquireReaderLock(Timeout.Infinite);
					if (rwLock.IsReaderLockHeld) try {
						foreach (WeakReference wr in buffers)
							if (wr.IsAlive)
								size += ((Buffer)wr.Target).Size;
					} finally {
						rwLock.ReleaseReaderLock();
					}
				}
				return size;
			}
		}

		/// <summary>�������� ��� ���������� ������������� ����.</summary>
		/// <remarks>��� �������� ������ ������ � ����, ��� ������ ����������� �� ��������
		/// �������� <see cref="Granularity"/>.
		/// <para>��������� ������������� ���� ������ ������ �� ����� ����������� ������. ��� ���������
		/// ������ �������� � ��� �� ��������.</para></remarks>
		/// <value>������������� ����.</value>
		/// <seealso cref="MaxBufferCount"/>
		public int Granularity {
			get { return granularity; }
			set { granularity = value; }
		}

		/// <summary>�������� ��� ���������� ������������ ���������� �������� �������.</summary>
		/// <remarks>��� ��������� ������������� ���������� ������� � ������� �������, �����
		/// ���������, ��� ���������� ������� � ���� ��� ���������� ��������� ����������. � ���� ������,
		/// ��� �� ����� ��������� ����� �������, �� ������ ����� ������������.</remarks>
		/// <value>������������ ���������� �������.</value>
		/// <seealso cref="Granularity"/>
		public int MaxBufferCount {
			get { return maxBufferCount; }
			set { maxBufferCount = value; }
		}

		/// <summary>Returns a <see cref="System.String"/> that represents the current <see cref="Object"/>.</summary>
		/// <value>A <see cref="System.String"/> that represents the current <see cref="Object"/>.</value>
		public override string ToString() {
			return String.Format(
					"{0}: Allocated {1} buffers, {2} Kb used.",
					this.GetType().Name, ActiveBuffers, UsedMemory / 1024);
		}

		static object managerLock = new object();
		static BufferPool defaultPool;
		/// <summary>�������� ��� ������� ��-���������.</summary>
		/// <remarks>��� ������� ��-��������� ��������� ��� ������ ��������� � ����� �������� � ����������
		/// ��� ����� �� ���������� ����������. ��������� ������������ �-�� Front Common Library � ���������
		/// ��������� ���������� ��� ��� �������� �����-������.<para>��� ������� ��-��������� �� ������������
		/// ������������� ���������� ������� � ����� ������������� 1024. ��� ��������� ����� ��������, ���������
		/// �������� �������� ������ <see cref="BufferPool"/>.</para></remarks>
		public static BufferPool DefaultPool {
			get {
				if (defaultPool == null) {
					Debug.Assert(managerLock != null);
					lock (managerLock) {
						if (defaultPool == null)
							defaultPool = new BufferPool();
						managerLock = null;
					}
				}
				return defaultPool;
			}
		}
	}

	/// <summary>��������������� �����, ����������� ����������� ������ ������ � <see cref="Stream"/>.</summary>
	public sealed class Streams {
		Streams() {}
		/// <summary>����������� ������ �� ������ <see cref="Stream"/> � ������.</summary>
		/// <param name="src">����� - �������� ������.</param>
		/// <param name="dst">����� - ���������� ������.</param>
		/// <param name="bp">��� �������, ������� ����� ����������� ��� �����������.</param>
		/// <param name="bufferSize">������ ������, ������� ����� ����������� ��� �����������.</param>
		public static void Copy(Stream src,
				Stream dst, BufferPool bp, int bufferSize) {
			BufferPool.Buffer buffer = null;
			if (bp != null) buffer = bp.Allocate(bufferSize, 0);
			if (buffer == null) buffer = new BufferPool.Buffer(null, bufferSize);
			using (buffer) {
				Copy(src, dst, buffer);
			}
		}

		/// <summary>����������� ������ �� ������ <see cref="Stream"/> � ������.</summary>
		/// <param name="src">����� - �������� ������.</param>
		/// <param name="dst">����� - ���������� ������.</param>
		/// <param name="buffer">����� ������� ����� ����������� ��� �����������.</param>
		public static void Copy(Stream src, Stream dst, byte[] buffer) {
			int length = buffer.Length;
			int readed;
			while ( (readed = src.Read(buffer, 0, length)) > 0 )
				dst.Write(buffer, 0, readed);
		}

		/// <summary>����������� ������ �� ������ <see cref="Stream"/> � ������.</summary>
		/// <param name="src">����� - �������� ������.</param>
		/// <param name="dst">����� - ���������� ������.</param>
		/// <remarks>��� ����������� ����� ����������� ����� �������� 8192 �����, ���������� �
		/// ���� ������� ��-���������.</remarks>
		public static void Copy(Stream src, Stream dst) {
			Copy(src, dst, BufferPool.DefaultPool, 8192);
		}
	}


	public class UnclosableStream : Stream {
		Stream stream;

		public UnclosableStream(Stream stream) {
			this.stream = stream;
		}

		public override void Close() {
			// ignore stream closing
		}

		public override void Flush() { stream.Flush(); }
		public override int Read(byte[] buffer, int offset, int count) {
			return stream.Read(buffer, offset, count);
		}
		public override int ReadByte() {
			return stream.ReadByte();
		}
		public override long Seek(long offset, SeekOrigin origin) {
			return stream.Seek(offset, origin);
		}
		public override void SetLength(long value) {
			stream.SetLength(value);
		}
		public override void Write(byte[] buffer, int offset, int count) {
			stream.Write(buffer, offset, count);
		}
		public override void WriteByte(byte value) {
			stream.WriteByte(value);
		}
		public override bool CanRead { get { return stream.CanRead; } }
		public override bool CanSeek { get { return stream.CanSeek; } }
		public override bool CanWrite { get { return stream.CanWrite; } }
		public override long Length { get { return stream.Length; } }
		public override long Position { get { return stream.Position; } set { stream.Position = value; } }
	}

}

