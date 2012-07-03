// $Id: Cache.cs 2368 2006-09-14 12:32:24Z kostya $

//#define PUBLISH_PERFORMANCE_COUNTERS

using System;
using System.Collections;
using System.Diagnostics;
using System.Threading;
using System.ComponentModel;

using Front.Diagnostics;

namespace Front {

	/// <summary>������, ���������� � ���� <see cref="Cache"/> ������ ������������ ���� ���������.</summary>
	public interface ICacheContainer {
		/// <summary>�������� ���������� ��� �������, ������������ <see cref="ICacheContainer"/>.</summary>
		void ClearCache();
		// TODO DF0010: ����� ��� ���-�� ��� ��� � �������� ����������� ��������?
		// TODO DF0078: �������� ������� �� � �����!
	}

	
	
	/// <summary>
	/// ����� <c>Cache</c> ������������ ��� ���������� �������� ������, ��������� ���������
	/// ������� �������� � ���������, �������� ������������ ��������� �����������.
	/// </summary>
	/// <remarks>
	/// <para>
	/// <c>Cache</c> �� ����� ������������ ��� �������� ������, ������� ������ �������� ��������,
	/// ��� ��� � ����� ������ ������� <c>Cache</c> ����� �������� ���� ���������� ��������� ���
	/// ��������.
	/// </para><para>
	/// <c>Cache</c> ������������ ��������� ���������� ������� ������: ����������� ������ � �������
	/// ��� �������� ��������� ������. ������� �������, ��� ��� ������, ������� <c>Cache</c> ������
	/// ��� ����� ������, ��� <see cref="WeakReference"/>, � ������ ����������� ������ �� ������ �� ��������
	/// ��������� ������.
	/// </para>
	/// </remarks>
	/// <threadsafety static="true" instance="true"/>
	/// <seealso cref="IEnvironment"/>
	// TODO: DF0011: ������� ��� ICacheContainer (done.)
	public class Cache : MarshalByRefObject, IEnumerable, IDisposable, ICacheContainer {
		///<summary> ���������, �������� ������� ���������� � ������ <see cref="Add" /> ��������,
		/// ��� �� ����������� �������� �� ��������� �������� ����������� �����������.</summary>
		public static readonly DateTime NoAbsoluteExpiration = DateTime.MaxValue;
		///<summary> ���������, �������� ������� ���������� � ������ <see cref="Add" /> ��������,
		/// ��� �� ����������� �������� �� ��������� �������� ����������� ��-���������.</summary>
		public static readonly TimeSpan NoSlidingExpiration = TimeSpan.Zero;

		public static readonly Front.Diagnostics.Log Log 
			= new Front.Diagnostics.Log(new TraceSwitch("Front.Cache", "Front.Cache", "Error"));
		
		/* -------------------------------------------------------------------------------- */
		
		static int			instanceNumber = 0;
		TimeSpan			defaultAbsolutePeriod = TimeSpan.FromMinutes(60);
		TimeSpan			defaultSlidingPeriod = TimeSpan.FromMinutes(15);

		ReaderWriterLock	rwlock = new ReaderWriterLock();
		object				cacheName;	// object to be compatible with Interlocked.Exchange(..)
		int					cleanPeriod;
		int					maxEntries;
		Hashtable			items = new Hashtable();
		Timer				timer;
		int					inOptimization = 0;
		
#if PUBLISH_PERFORMANCE_COUNTERS
		static Cache() {
			if (!PerformanceCounterCategory.Exists("Front Cache")) {
				CounterCreationData hits = new CounterCreationData();
				hits.CounterName = "Hit Count";
				hits.CounterType = PerformanceCounterType.NumberOfItems32;
				CounterCreationData cacheSize = new CounterCreationData();
				cacheSize.CounterName = "Cache Size";
				cacheSize.CounterType = PerformanceCounterType.NumberOfItems32;
				CounterCreationDataCollection ccds = new CounterCreationDataCollection();
				ccds.Add(hits);
				ccds.Add(cacheSize);
				PerformanceCounterCategory.Create("Front Cache", "Front Cache counters", ccds); 
			}
		}
#endif

		/// <summary>������� ��������� <see cref="Cache"/> � �������� ����������� 60 ������ �
		/// ��� ����������� ������������� ���������� ���������. </summary>
		public Cache():this(60) {}
		
		/// <summary>������� ��������� <see cref="Cache"/> � ��������� �������� ����������� �
		/// ��� ����������� ������������� ���������� ���������. </summary>
		/// <remarks>���� ��������� ��������
		/// ������ 5 ������, ������ ����������� ����������� ������ 5 ��������.</remarks>
		/// <param>������ ����������� <c>Cache</c>. ����������� � ��������.</param>
		public Cache(int cleanPeriod):this(cleanPeriod, 0) { }

		/// <summary>������� ��������� <see cref="Cache"/> � ��������� �������� ����������� �
		/// ������������ ����������� ���������. </summary>
		/// <remarks>���� ��������� ��������
		/// ������ 5 ������, ������ ����������� ����������� ������ 5 ��������.</remarks>
		/// <param name="cleanPeriod">������ ����������� <c>Cache</c>. ����������� � ��������.</param>
		/// <param name="maxEntries">������������ ���������� ���������. ��� �������� 0, ����������� ���.</param>
		public Cache(int cleanPeriod, int maxEntries):this(null, cleanPeriod, maxEntries) { }

		/// <summary>������� ����������� ��������� <see cref="Cache"/> � ��������� �������� ����������� �
		/// ������������ ����������� ���������. </summary>
		/// <remarks>���� ��������� ��������
		/// ������ 5 ������, ������ ����������� ����������� ������ 5 ��������.</remarks>
		/// <param name="name">��� ��� ������������ ���������� <c>Cache</c>.</param>
		/// <param name="cleanPeriod">������ ����������� <c>Cache</c>. ����������� � ��������.</param>
		/// <param name="maxEntries">������������ ���������� ���������. ��� �������� 0, ����������� ���.</param>
		public Cache(string name, int cleanPeriod, int maxEntries) {
			this.cleanPeriod = (cleanPeriod < 5) ? 5 : cleanPeriod;
			this.maxEntries = maxEntries;
			try {
				this.Name = (name != null && name.Length > 0)
					? name
					: Process.GetCurrentProcess().ProcessName + Interlocked.Increment(ref instanceNumber).ToString();
			} catch (InvalidOperationException ex) {
				// catch this case: http://msdn.microsoft.com/netframework/programming/bcl/faq/SystemDiagnosticsProcessFAQ.aspx#Question2
				this.Name = "buggedProcess" + Interlocked.Increment(ref instanceNumber).ToString();
			}
			Log.Info(RM.GetString("LogCacheCreated"), Name, cleanPeriod, maxEntries);
		}

		/// <summary>����������� ������������� �������, ������������ <see cref="Cache"/>.</summary>
		~Cache() {
			Dispose(false);
		}

		/// <summary>����������� ����������� � ������������� �������, ������������ <see cref="Cache"/>.</summary>
		public void Dispose() { Dispose(true); }

		/// <summary>����������� ����������� � ������������� �������, ������������ <see cref="Cache"/>.</summary>
		/// <remarks>���� ����� ���������� �� <see cref="IDisposable.Dispose"/> � �� ������
		/// <see cref="Finalize"/> � ���������� <c>disposing</c> ������������� � <c>true</c> � <c>false</c>
		/// ��������������.</remarks>
		protected virtual void Dispose(bool disposing) {
			if (disposing) {
				rwlock.AcquireWriterLock(Timeout.Infinite);
				try {
					if (items != null) {
						GC.SuppressFinalize(this);
						items = null;
#if PUBLISH_PERFORMANCE_COUNTERS
						if (hitCounter != null) {
							hitCounter.RemoveInstance();
							cacheSize.RemoveInstance();
							hitCounter.Close();
							cacheSize.Close();
						}
#endif
					}
				} finally {
					rwlock.ReleaseWriterLock();
				}
			}
		}

		/// <summary>��������� ���� �� � <see cref="Cache"/> ������� � ��������� ������.</summary>
		/// <remarks>������� �������, ��� ������������� ����� ����� ������ �� �����������, ��� �������
		/// ������������� � <c>Cache</c> ��� �����-�� �����. �� ����� ���� ������ ��� ��.</remarks>
		/// <param name="code"> ��� �������� ��������.</param>
		/// <exception cref="ArgumentNullException">�������� <c>code</c> ����� <c>null</c>.</exception>
		/// <exception cref="ObjectDisposedException">��� ������� ���������� ���
		/// ������ ����� <see cref="Dispose"/>.</exception>
		public bool Contains(string code) {
			if (code == null) throw new ArgumentNullException("code");
			rwlock.AcquireReaderLock(Timeout.Infinite);
			try {
#if PUBLISH_PERFORMANCE_COUNTERS
				hitCounter.Increment();
#endif
				if (items == null) throw new ObjectDisposedException(this.GetType().Name);
				return items.Contains(code);
			} finally {
				rwlock.ReleaseReaderLock();
			}
		}

		/// <summary>��������� � <see cref="Cache"/> ������� <c>value</c> � ������ <c>code</c>.</summary>
		/// <remarks><para>���� ������� � ����� ������ ��� ���������� �� ����� �������.</para>
		/// <para>����� ����������� ����������� ��������� ������������ ���� ����� ��������. ��� ������ ��
		/// �����������, ������������ ����� ����� ����� ������ ����� ������.</para>
		/// <para>����� �������������� ����������� ��������� ���� ����� �������� ����� ���������� � ���� ���������.
		/// ���� ���� ����� ��������� � �������� ������ <c>slidingExpiration</c>, ������� ��������� ����
		/// ���� ����� ����������� ����������� ��� �� ����������.</para>
		/// <para>��������� ��������� <c>absoluteExpiration</c> � �������� <see cref="NoAbsoluteExpiration"/>,
		/// ��������� �������� ����������� ����������� ��� ������� ��������.</para>
		/// <para>��������� ��������� <c>slidingExpiration</c> � �������� <see cref="NoSlidingExpiration"/>,
		/// ��������� �������� �������������� ����������� ��� ������� ��������.</para>
		///</remarks>
		/// <param name="code">��� ������ ��������.</param>
		/// <param name="value">�������� ������ ��������.</param>
		/// <param name="absoluteExpiration">����� ����������� ����������� ������ ��������.</param>
		/// <param name="slidingExpiration">������ �������������� �����������</param>
		/// <exception cref="ArgumentNullException">�������� <c>code</c> ����� <c>null</c>.</exception>
		/// <exception cref="ObjectDisposedException">��� ������� ���������� ���
		/// ������ ����� <see cref="Dispose"/>.</exception>
		/// <seealso cref="this"/>
		/// <seealso cref="Get"/>
		/// <seealso cref="Remove"/>
		public virtual object Add(string code, object value,
				DateTime absoluteExpiration, TimeSpan slidingExpiration) {
			if (code == null) throw new ArgumentNullException("code");
			rwlock.AcquireWriterLock(Timeout.Infinite);
			try {
				if (items == null) throw new ObjectDisposedException(this.GetType().Name);
				if (maxEntries > 0 && items.Count >= maxEntries)
					RemoveOneEntry();
				items[code] = new CacheEntry(value, absoluteExpiration, slidingExpiration);
				Log.Info(RM.GetString("LogCacheAdded"), this.Name, code, value);
				if (timer == null)
					timer = new Timer(new TimerCallback(Optimize), null, cleanPeriod * 1000, cleanPeriod * 1000);
#if PUBLISH_PERFORMANCE_COUNTERS
				cacheSize.RawValue = this.Count;
#endif
			} finally {
				rwlock.ReleaseWriterLock();
			}
			return value;
		}

		/// <summary>��������� � <see cref="Cache"/> ������� <c>value</c> � ������ <c>code</c>.</summary>
		/// <remarks><para>���� ������� � ����� ������ ��� ���������� �� ����� �������.</para>
		/// <para>����� ����������� ����������� ��������� ������������ ���� ����� ��������. ��� ������ ��
		/// �����������, ������������ ����� ����� ����� ������ ����� ������.</para>
		/// <para>����� �������������� ����������� ��������� ���� ����� �������� ����� ���������� � ���� ���������.
		/// ���� ���� ����� ��������� � �������� ������ <c>slidingExpiration</c>, ������� ��������� ����
		/// ���� ����� ����������� ����������� ��� �� ����������.</para>
		/// <para>����� ����������� ����������� � ������������ ����������� � �������������� �������
		/// <see cref="DefaultAbsolutePeriod"/> � <see cref="DefaultSlidingPeriod"/> ��������������.</para>
		///</remarks>
		/// <param name="code">��� ������ ��������.</param>
		/// <param name="value">�������� ������ ��������.</param>
		/// <exception cref="ArgumentNullException">�������� <c>code</c> ����� <c>null</c>.</exception>
		/// <exception cref="ObjectDisposedException">��� ������� ���������� ���
		/// ������ ����� <see cref="Dispose"/>.</exception>
		/// <seealso cref="this"/>
		/// <seealso cref="Get"/>
		/// <seealso cref="Remove"/>
		public object Add(string code, object value) {
			return this.Add(code, value, DateTime.Now + defaultAbsolutePeriod, defaultSlidingPeriod);
		}

		void RemoveOneEntry() {
			Log.Verb(RM.GetString("LogCacheMaxExceed"), this.Name);
			rwlock.AcquireWriterLock(Timeout.Infinite);
			try {
				if (items == null) throw new ObjectDisposedException(this.GetType().Name);
				DateTime expireTime = DateTime.MaxValue;
				string next2Expire = null;
				foreach (string key in items.Keys) {
					CacheEntry entry = (CacheEntry)items[key];
					DateTime et = entry.ExpirationTime;
					if (et < expireTime) {
						expireTime = et;
						next2Expire = key;
					}
				}
				if (next2Expire != null) InternalRemove(next2Expire);
			} finally {
				rwlock.ReleaseWriterLock();
			}
		}

		/// <summary>������� ������� � ������ <c>code</c> �� <see cref="Cache"/>.</summary>
		/// <remarks>���� ������� � ����� ������ �� ������, �������� ������������.</remarks>
		/// <param name="code">��� ��������.</param>
		/// <exception cref="ArgumentNullException">�������� <c>code</c> ����� <c>null</c>.</exception>
		/// <exception cref="ObjectDisposedException">��� ������� ���������� ���
		/// ������ ����� <see cref="Dispose"/>.</exception>
		public virtual object Remove(string code) {
			if (code == null) throw new ArgumentNullException("code");
			rwlock.AcquireReaderLock(Timeout.Infinite);
			try {
				if (items == null) throw new ObjectDisposedException(this.GetType().Name);
				object o = items[code];
				if (o != null) {
					rwlock.UpgradeToWriterLock(Timeout.Infinite);
					InternalRemove(code);
					items.Remove(code);
#if PUBLISH_PERFORMANCE_COUNTERS
					cacheSize.RawValue = this.Count;
#endif
					return ((CacheEntry)o).Value;
				} else
					return null;
			} finally {
				rwlock.ReleaseReaderLock();
			}
		}

		void InternalRemove(string code) {
			Log.Info(RM.GetString("LogCacheRemove"), this.Name, code);
			items.Remove(code);
		}

		/// <summary>������ ��� ������������� ������� � ������ <c>code</c>.</summary>
		/// <remarks>���� ������� ������� �� ������, ������������ �������� <c>null</c>.
		/// ��� ���������� �������� ������������ �������� <see cref="DefaultAbsolutePeriod"/>
		/// � <see cref="DefaultSlidingPeriod"/>.</remarks>
		/// <param name="code">��� ��������.</param>
		/// <exception cref="ArgumentNullException">�������� <c>code</c> ����� <c>null</c>.</exception>
		/// <exception cref="ObjectDisposedException">��� ������� ���������� ���
		/// ������ ����� <see cref="Dispose"/>.</exception>
		/// <seealso cref="Add"/>
		/// <seealso cref="Get"/>
		/// <seealso cref="Remove"/>
		public object this[string code] {
			get { return Get(code); }
			set { Add(code, value); }
		}
		
		/// <summary>������ ������� � ������ <c>code</c>.</summary>
		/// <remarks>���� ������� ������� �� ������, ������������ �������� <c>null</c>.</remarks>
		/// <param name="code">��� ��������.</param>
		/// <exception cref="ArgumentNullException">�������� <c>code</c> ����� <c>null</c>.</exception>
		/// <exception cref="ObjectDisposedException">��� ������� ���������� ���
		/// ������ ����� <see cref="Dispose"/>.</exception>
		/// <seealso cref="Add"/>
		/// <seealso cref="this"/>
		/// <seealso cref="Remove"/>
		public virtual object Get(string code) {
			if (code == null) throw new ArgumentNullException("code");
			object o;
			rwlock.AcquireReaderLock(Timeout.Infinite);
			try {
				if (items == null) throw new ObjectDisposedException(this.GetType().Name);
#if PUBLISH_PERFORMANCE_COUNTERS
				hitCounter.Increment();
#endif
				o = items[code];
			} finally {
				rwlock.ReleaseReaderLock();
			}
			return (o == null) ? null : ((CacheEntry)o).Value;
		}

		/// <summary>������� <see cref="Cache"/>.</summary>
		/// <remarks>��� �������� ���������.</remarks>
		/// <exception cref="ObjectDisposedException">��� ������� ���������� ���
		/// ������ ����� <see cref="Dispose"/>.</exception>
		/// <seealso cref="Remove"/>
		public virtual void Clear() {
			Log.Info(RM.GetString("LogCacheMaxExceed"), this.Name);
			rwlock.AcquireWriterLock(Timeout.Infinite);
			try {
				if (items == null) throw new ObjectDisposedException(this.GetType().Name);
				if (timer != null) {
					timer.Dispose();
					timer = null;
				}
				items.Clear();
#if PUBLISH_PERFORMANCE_COUNTERS
				cacheSize.RawValue = this.Count;
#endif
			} finally {
				rwlock.ReleaseWriterLock();
			}
		}

		public virtual void ClearCache() { this.Clear(); }
		
		/// <summary>������������ <see cref="Cache"/>.</summary>
		/// <remarks>���� ����� ������ ���������� ����� <c>Cache</c> � �������������� ��������
		/// � <see cref="CleanPeriod"/>. �� ������� ���������� �������� � ������� ��.</remarks>
		/// <exception cref="ObjectDisposedException">��� ������� ���������� ���
		/// ������ ����� <see cref="Dispose"/>.</exception>
		/// <seealso cref="Remove"/>
		/// <seealso cref="Clear"/>
		protected virtual void Optimize(object state) {
			int optimizeActive = Interlocked.CompareExchange(ref inOptimization, 1, 0);
			if (optimizeActive == 0) try {
				Log.Info(RM.GetString("LogCacheOptimize"), this.Name);
				DateTime n = (state == null) ? DateTime.Now : (DateTime)state;
				string[] keys;
				rwlock.AcquireReaderLock(Timeout.Infinite);
				try {
					if (items == null) return;
					keys = new string[items.Keys.Count];
					items.Keys.CopyTo(keys, 0);
					foreach (string key in keys) {
						CacheEntry ce = (CacheEntry)items[key];
						if (ce != null && ce.ExpirationTime < n) {
							if (!rwlock.IsWriterLockHeld) rwlock.UpgradeToWriterLock(Timeout.Infinite);
							this.InternalRemove(key);
						}
					}
#if PUBLISH_PERFORMANCE_COUNTERS
					cacheSize.RawValue = this.Count;
#endif
				} finally {
					rwlock.ReleaseReaderLock();
				}
			} finally {
				Interlocked.Decrement(ref inOptimization);
				Log.Info(RM.GetString("LogCacheOptimized"), this.Name);
			}
		}

		/// <summary>���������� ��������� � <see cref="Cache"/>.</summary>
		/// <exception cref="ObjectDisposedException">��� ������� ���������� ���
		/// ������ ����� <see cref="Dispose"/>.</exception>
		/// <seealso cref="Add"/>
		/// <seealso cref="Remove"/>
		/// <seealso cref="this"/>
		public int Count {
			get {
				rwlock.AcquireReaderLock(Timeout.Infinite);
				try {
					if (items == null) throw new ObjectDisposedException(this.GetType().Name);
					return items.Count;
				} finally {
					rwlock.ReleaseReaderLock();
				}
			}
		}

		/// <summary>������� <see cref="IEnumerator"/> ��� �������� ��������� <see cref="Cache"/>.</summary>
		/// <remarks>������� ��������� ���� - ������� ��������. �� ����� �������� �������� ���� �����
		/// ���� �������. � ������ ������, <c>IEnumerator</c>, ����������� ���� �������, �� ������ ������
		/// ��� ����, ����� ������ ��������� ���� � �������� ������. �� � �������, ��� ������ ����� ���������.
		/// <para>����� ������������� ������������ ���� ����������� ������ <see cref="Lock"/> � <see cref="Unlock"/>.
		/// </para></remarks>
		/// <exception cref="ObjectDisposedException">��� ������� ���������� ���
		/// ������ ����� <see cref="Dispose"/>.</exception>
		/// <seealso cref="this"/>
		public IEnumerator GetEnumerator() {
			if (items == null) throw new ObjectDisposedException(this.GetType().Name);
			return new Enumerator(items.GetEnumerator());
		}

		/// <summary>��������� ������ � <see cref="Cache"/>.</summary>
		/// <remarks>����� ��������� ���� ����� ��������� ������ � ��� ����� �������, ��� ������
		/// ��� ����� � ���� ������. ��� ��������, ��� ���� ����� ��� �� ������ ���, �� ����� ������ ���
		/// ���� �� ������. ������� �������� ������������ <see cref="Cache"/>
		/// �� ���������� ������, ������ ��� ���� ��� ������������ ������ ������ �� ����� � ���� ������, � ��� ����� �
		/// �����, ����������� ����������� ����. </remarks>
		/// <seealso cref="Unlock"/>
		/// <seealso cref="GetEnumerator"/>
		public void Lock() {
			rwlock.AcquireReaderLock(Timeout.Infinite);
		}

		/// <summary>������� ���������� ������ � <see cref="Cache"/>.</summary>
		/// <seealso cref="Lock"/>
		/// <seealso cref="GetEnumerator"/>
		public void Unlock() {
			rwlock.ReleaseReaderLock();
		}

		/// <summary>Gets the current sequence number.</summary>
		/// <value>The current sequence number.</value>
		/// <remarks><para>The sequence number increases whenever a thread acquires the writer lock.
		/// You can save the sequence number and pass it to AnyWritersSince at a later time, if
		/// you want to determine whether other threads have acquired the writer lock in the meantime.</para>
		/// <para>You can use WriterSeqNum to improve application performance. For example, a thread might
		/// cache the information it obtains while holding a reader lock. After releasing and later reacquiring
		/// the lock, the thread can determine whether other threads have written to the resource by calling
		/// <see cref="AnyWritersSince"/>; if not, the cached information can be used. This technique is useful
		/// when reading the information protected by the lock is expensive; for example,
		/// running a database query.</para>
		/// <para>The caller must be holding a reader lock or a writer lock in order for the sequence
		/// number to be useful.</para>
		///</remarks>
		/// <seealso cref="Lock"/>
		/// <seealso cref="Unlock"/>
		/// <seealso cref="AnyWritersSince"/>
		public int WriterSeqNum {
			get {
				return rwlock.WriterSeqNum;
			}
		}

		/// <summary>Indicates whether the writer lock has been granted to any thread since the sequence
		/// number was obtained.</summary>
		/// <returns>true if the writer lock has been granted to any thread since the sequence number
		/// was obtained; otherwise, false.</returns>
		/// <remarks><para>You can use WriterSeqNum and AnyWritersSince to improve application
		/// performance. For example, a thread might cache the information it obtains while holding
		/// a reader lock. After releasing and later reacquiring the lock, the thread can use
		/// AnyWritersSince to determine whether other threads have written to the resource in the
		/// interim; if not, the cached information can be used. This technique is useful where
		/// reading the information protected by the lock is expensive; for example, running a database query.</para>
		/// <para>The caller must be holding a reader lock or a writer lock in order for the sequence
		/// number to be useful.</para>
		///</remarks>
		/// <seealso cref="Lock"/>
		/// <seealso cref="Unlock"/>
		/// <seealso cref="WriterSeqNum"/>
		public bool AnyWritersSince(int seqNum) {
			return rwlock.AnyWritersSince(seqNum);
		}

		/// <summary>��������� ��� ���������� ������������� ���������� ����������� <see cref="Cache"/>.</summary>
		/// <value>������������� ���������� ����������� � ��������. </value>
		/// <remarks>���� ��������� ��������
		/// ������ 5 ������, ������ ����������� ����������� ������ 5 ��������.</remarks>
		/// <exception cref="ObjectDisposedException">���������� set, ����� ����� ��� ������� ���������� ���
		/// ������ ����� <see cref="Dispose"/>.</exception>
		/// <seealso cref="InOptimization"/>
		public int CleanPeriod {
			get { return cleanPeriod; }
			set {
				rwlock.AcquireReaderLock(Timeout.Infinite);
				try {
					if (items == null) throw new ObjectDisposedException(this.GetType().Name);
					cleanPeriod = (value < 5) ? 5 : value;
					if (timer != null) {
						rwlock.UpgradeToWriterLock(Timeout.Infinite);
						timer.Change(cleanPeriod * 1000, cleanPeriod * 1000);
					}
				} finally {
					rwlock.ReleaseReaderLock();
				}
			}
		}

		/// <summary>���������� ���������� ����������� ��� ����� ���������� <see cref="Cache"/>.</summary>
		/// <value>true ���� ����������� ������� � false, ���� ���. </value>
		/// <seealso cref="CleanPeriod"/>
		public bool InOptimization {
			get {
				return inOptimization > 0;
			}
		}

		/// <summary>��������� ��� ���������� ��� ��� ����������� ���������� <see cref="Cache"/>.</summary>
		/// <value>��� ����������.</value>
#if PUBLISH_PERFORMANCE_COUNTERS
		/// <exception cref="ObjectDisposedException">������ set � ��� ������� ���������� ���
		/// ������ ����� <see cref="Dispose"/>.</exception>
#endif
		public string Name {
			get { return (string)cacheName; }
			set {
#if PUBLISH_PERFORMANCE_COUNTERS
				rwlock.AcquireWriterLock(Timeout.Infinite);
				try {
					if (items == null) throw new ObjectDisposedException(this.GetType().Name);
					Interlocked.Exchange(ref cacheName, value);
					if (hitCounter != null) {
						hitCounter.RemoveInstance();
						cacheSize.RemoveInstance();
						hitCounter.InstanceName = cacheSize.InstanceName = value;
					} else {
						hitCounter = new PerformanceCounter("Front Cache", "Hit Count", value, false);
						cacheSize = new PerformanceCounter("Front Cache", "Cache Size", value, false);
						hitCounter.RawValue = cacheSize.RawValue = 0;
					}
				} finally {
					rwlock.ReleaseWriterLock();
				}
#else
				Interlocked.Exchange(ref cacheName, value);
#endif
			}
		}

#if PUBLISH_PERFORMANCE_COUNTERS
		PerformanceCounter hitCounter;
		PerformanceCounter cacheSize;
#endif

		/// <summary>��������� ��� ���������� ������ ����������� ����������� ��-���������.</summary>
		/// <value>������ ����������� ����������� ��-���������.</value>
		/// <remarks>��-���������, ������ ����������� ����������� ����� 60 �����.</remarks>
		public TimeSpan DefaultAbsolutePeriod {
			get { return defaultAbsolutePeriod; }
			set { defaultAbsolutePeriod = value; }
		}

		/// <summary>��������� ��� ���������� ������ �������������� ����������� ��-���������.</summary>
		/// <value>������ �������������� ����������� ��-���������.</value>
		/// <remarks>��-���������, ������ �������������� ����������� ����� 15 �������.</remarks>
		public TimeSpan DefaultSlidingPeriod {
			get { return defaultSlidingPeriod; }
			set { defaultSlidingPeriod = value; }
		}

		///<summary>���� ����� ������������ ��� ����������� ������������� ������� <see cref="Cache"/></summary>
		protected class CacheEntry {
			DateTime		expire;
			//WeakReference	value;
			object		value;

			///<summary>���� ����� ������������ ��� ����������� ������������� ������� <see cref="Cache"/></summary>
			public CacheEntry(object value, DateTime absoluteExpiration, TimeSpan slidingExpiration) {
				this.value = value; //new WeakReference(value);
				AbsoluteExpiration = absoluteExpiration;
				SlidingExpiration = slidingExpiration;
				expire = DateTime.Now + slidingExpiration;
				if (expire > absoluteExpiration) expire = absoluteExpiration;
			}
			
			///<summary>���� ����� ������������ ��� ����������� ������������� ������� <see cref="Cache"/></summary>
			public object Value { get {
				//if (value.IsAlive) {
				if (value != null) {
					DateTime newExpire = DateTime.Now + SlidingExpiration;
					expire = (newExpire < AbsoluteExpiration) ? newExpire : AbsoluteExpiration;
					return value; //value.Target;
				} else {
					expire = DateTime.MinValue;
					value = null; // ��� ������ weakreference
					return null;
				}
			} }

			///<summary>���� ����� ������������ ��� ����������� ������������� ������� <see cref="Cache"/></summary>
			public DateTime ExpirationTime { get {
				return expire;
			} }

			///<summary>���� ����� ������������ ��� ����������� ������������� ������� <see cref="Cache"/></summary>
			public readonly DateTime AbsoluteExpiration;
			///<summary>���� ����� ������������ ��� ����������� ������������� ������� <see cref="Cache"/></summary>
			public readonly TimeSpan SlidingExpiration;
		}

		///<summary>���� ����� ������������ ��� ����������� ������������� ������� <see cref="Cache"/></summary>
		public class Enumerator : IDictionaryEnumerator {
			private IDictionaryEnumerator _e;
			///<summary>���� ����� ������������ ��� ����������� ������������� ������� <see cref="Cache"/></summary>
			public Enumerator (IDictionaryEnumerator e) { _e = e; }
			///<summary>���� ����� ������������ ��� ����������� ������������� ������� <see cref="Cache"/></summary>
			public void Reset() { _e.Reset(); }
			///<summary>���� ����� ������������ ��� ����������� ������������� ������� <see cref="Cache"/></summary>
			public bool MoveNext() { return _e.MoveNext(); }
			///<summary>���� ����� ������������ ��� ����������� ������������� ������� <see cref="Cache"/></summary>
			public object Current { get { return this.Entry;	} }
			///<summary>���� ����� ������������ ��� ����������� ������������� ������� <see cref="Cache"/></summary>
			public DictionaryEntry Entry {get {
				DictionaryEntry de = _e.Entry;
				de.Value = ((CacheEntry)de.Value).Value;
				return de;
			} }
			///<summary>���� ����� ������������ ��� ����������� ������������� ������� <see cref="Cache"/></summary>
			public object Key {get { return this.Entry.Key; } }
			///<summary>���� ����� ������������ ��� ����������� ������������� ������� <see cref="Cache"/></summary>
			public object Value {get { return this.Entry.Value; } }
		}
		
	}
}

