using System;
using System.Collections;
using System.Data;
using System.Text;
using System.Collections.Generic;
using System.Threading;

namespace Front.Processing {

	public class ProcessingQueueItem<C> {
		protected C InnerItem;
		protected QueueItemState InnerState;
		protected int InnerPriority;
		protected int InnerDelay;
		protected long InnerKey;

		public ProcessingQueueItem(long key, C item, int priority) {
			InnerKey = key;
			InnerItem = item;
			InnerState = QueueItemState.Waiting;
			InnerPriority = priority;
		}

		public C Item { 
			get { return InnerItem; } 
		}

		public QueueItemState State { 
			get { return InnerState; }
			set { InnerState = value; }
		}

		public int Priority { 
			get { return InnerPriority; } 
		}

		public int Delay {
			get { return InnerDelay; }
			set { InnerDelay = value; }
		}

		public long Key { 
			get { return InnerKey; } 
		}
	}

	// TODO: ��� ������ ���� ������� � ������������
	// TODO: ����� ��������� �� ������� ����������� ��� ���������� ��� ������!

	/// <summary>
	/// ������� ���������. ������������ ������� ���������� � �� ���������� � �������.
	/// </summary>
	/// <typeparam name="T">��� ������� ���������.</typeparam>
	public class ProcessingQueue<T> : IDisposable {

		public static int DefaultTimeOut = 10000;

		#region Protected Properties
		//.............................................................................
		//protected List<T> Items;
		protected List<ProcessingQueueItem<T>> InnerItems;
		//protected List<long> Keys;
		protected int InnerTimeOut;
		protected int InnerDelay;
		protected bool InnerRetry = false;
		protected int InnerMaxRetryCount;
		protected long KeysCounter;
		protected QueueState InnerState = QueueState.None;
		protected EventWaitHandle InnerQueueEvent;

		protected const int threadCount = 3;
		//��������� ������� ��������� ���������� � ���� �������(�� 1..threadCount)
		protected List<Thread> InnerQueueProcessorThreads = new List<Thread>();
		//protected Thread InnerQueueProcessorThread;

		protected Thread InnerItemProcessThread;
		protected int InnerSleepTime = 100;
		//.............................................................................
		#endregion

		public ProcessingQueue() : this (DefaultTimeOut) {
		}

		public ProcessingQueue(int timeout) {
			InnerTimeOut = timeout;
			InnerItems = new List<ProcessingQueueItem<T>>();

			InnerQueueEvent = new EventWaitHandle(false, EventResetMode.ManualReset);

			//InnerQueueProcessorThread = new Thread(new ThreadStart(Run));
			for (int i = 0; i < threadCount; i++) {
				Thread thread = new Thread(new ThreadStart(Run));
				InnerQueueProcessorThreads.Add(thread);
				//������ ��� ������ ��������, ��� ����, ����� ����� ���������� ��������� ��� ��������� ����� �� ��������� ��������
				thread.IsBackground = true;
			}

			InnerItemProcessThread = new Thread(new ThreadStart(DoTryProcess));
		}

		public virtual void Dispose() {
			Stop();
			Clear();
		}

		public event EventHandler<QueueEventArgs> AfterEnqueue;
		public event EventHandler<QueueEventArgs> AfterDequeue;
		public event EventHandler<QueueEventArgs> BeforeProcess;
		public event EventHandler<QueueErrorEventArgs> AfterError;
		public event EventHandler<QueueEventArgs> AfterProcessingTimedOut;


		#region Public Properties
		//.............................................................................
		public int Count { 
			get { return InnerItems.Count;  } 
		}

		public virtual long CurrentKey { 
			get {
				ProcessingQueueItem<T> item = CurrentItem;
				if (item == null) return 0;
				return item.Key;
			} 
		}

		public virtual T Current { 
			get {
				ProcessingQueueItem<T> item = CurrentItem;
				if (item == null) return default(T);
				return item.Item;
			} 
		}

		public int Timeout { 
			get { return InnerTimeOut; }
			set { InnerTimeOut = value; }
		}

		// TODO: �������� ���������� ������ � ������������� ������!
		public int MaxRetrycount {
			get { return InnerMaxRetryCount; }
			set { 
				if (value <= 0 )
					Error.Warning(new ArgumentException("value"), typeof(Queue));
				else
					InnerMaxRetryCount = value;
			}
		}

		public List<T> Items {
			get {
				List<T> items = new List<T>();
				lock (InnerItems) {
					foreach (ProcessingQueueItem<T> item in InnerItems) items.Add(item.Item);
				}
				return items;
			}
		}
		//.............................................................................
		#endregion


		#region Public Methods
		//.............................................................................
		public virtual long Enqueue(T item) {
			return Enqueue(item, 0);
		}

		public virtual long Enqueue(T item, int priority) {
			long nk = 0;
			lock (this) {
				KeysCounter++;
				nk = KeysCounter;
				ProcessingQueueItem<T> newItem = new ProcessingQueueItem<T>(nk, item, priority);
				InnerItems.Add(newItem);

				lock (InnerQueueEvent)
					InnerQueueEvent.Set();
			}
			OnAfterEnqueue(nk, item);
			return nk;
		}

		public virtual bool Discard(long ItemKey) {
			ProcessingQueueItem<T> item = GetItem(ItemKey);
			if (item == null) return false;

			InnerItems.Remove(item);
			return true;
		}

		public virtual bool Pause(long ItemKey) {
			ProcessingQueueItem<T> item = GetItem(ItemKey);
			if (item == null ) return false;
			item.State = QueueItemState.Paused;
			return true;
		}

		public virtual bool Resume(long ItemKey) {
			ProcessingQueueItem<T> item = GetItem(ItemKey);
			if (item == null) return false;
			item.State = QueueItemState.Waiting;
			return true;
		}

		public virtual void Retry() {
			// TODO: ����� �������������, ��� Retry ���������� � ��� �� ������, ��� � TryProcess!
			InnerRetry = true;
		}

		public virtual QueueItemState GetState(long ItemKey) {
			ProcessingQueueItem<T> item = GetItem(ItemKey);
			if (item == null) return QueueItemState.NotExists;
			return item.State;
		}

		// TODO: ����� ����� ���� ����������� �������� Item'� �� ItemKey, ����������� ������ ��� ��� ���
		// TODO: ����� ���������� "��������� item'�" � �����-�� ������ � ������ � ����� ������ ������,
		// � ��� ����� � ���������� Item �� ��������� � ����� � ������� (���� ��� ����� ������� � ��
		// ��������� �������!)

		public virtual void Start() {
			InnerState = QueueState.Working;
			//InnerQueueProcessorThread.Start();
			foreach (Thread thread in InnerQueueProcessorThreads)
				thread.Start();
		}

		public virtual void Stop() {
			InnerState = QueueState.Stopped;
			foreach (Thread thread in InnerQueueProcessorThreads)
				thread.Abort();
			//InnerQueueProcessorThread.Abort();
		}

		public virtual bool Clear() {
			// ������ ���������� ������ ��� ������������� �������
			if (InnerState == QueueState.Working) return false;
			InnerItems.Clear();
			return true;
		}
		//.............................................................................
		#endregion


		#region Protected Methods
		//.............................................................................

		protected virtual ProcessingQueueItem<T> CurrentItem {
			get {
				ProcessingQueueItem<T> res = null;
				lock (InnerItems) {
					if (InnerItems.Count == 0)
						return null;
					for (int i = 0; i < InnerItems.Count; i++) {
						if (InnerItems[i]!= null && InnerItems[i].Delay == 0
							&& (InnerItems[i].State == QueueItemState.Waiting
								|| InnerItems[i].State == QueueItemState.Processing
								|| InnerItems[i].State == QueueItemState.Processed)) {
							res = InnerItems[i];
							break;
						}
					}
				}
				return res;
			}
		}

		protected virtual ProcessingQueueItem<T> Shift() {
			long key = CurrentKey;
			if (key == 0) return null;
			ProcessingQueueItem<T> item = GetItem(key);
			if (item == null) return null;

			InnerItems.Remove(item);
			return item;
		}

		protected void Run() {
			// TODO: ��� ����� ����������! ���� ������ ������������ ������ �� �������, � �� ��������� - ��������
			// ��� ����������� ����� ������ - ����� ��������� (��� �����) � ������� ������, ����� ����� ��������!
			// �������� ����� ���� ����� ���������, ��� �� ������ "���������", �� � ������ 0-��������
			// ��� ������ ��������.
			// ��� ���������, �� ������ - ���������(����) �� �������� ���!

			while (true) {
				//Thread.Sleep(InnerSleepTime);
				InnerQueueEvent.WaitOne();
				Process();
			}
		}

		// TODO: ������� threadSafe...
		protected virtual void Process() {
			//XXX: ��-�� ����, ��� ��� ������� ����� ����������� ����� �������, �� ��������� ������ � �������
			//��� �� ����� ������ - ������������� ������� ���� ����
			//�������� �� ����������� ������������� ������������� �������(��-�� ������� ������ ������� ��� �����������)
			lock (this) {
				if (this.Count == 0) return;

				int myMaxRetry = MaxRetrycount;
				EventHandler<QueueEventArgs> h = BeforeProcess;
				QueueEventArgs args;
				if (h != null) {
					args = new QueueEventArgs(CurrentKey, Current);
					h(this, args);
				}

				int RetryCount = 0;
				do {
					InnerRetry = false;
					int delay = 0;
					ProcessingQueueItem<T> currentItem;
					currentItem = CurrentItem;
					try {

						//�������� ��������� � ����� ������
						//Thread processThread = new Thread(new ThreadStart(DoTryProcess));
						//processThread.Start();
						//XXX : ����������, ��� �� ������ � ����� � ��� �� ������ ��������� ���������
						//���� ����� �������� ���� ������, �� ����� �������� ��������� ��� ��������� -
						//�������� exception
						InnerItemProcessThread = new Thread(new ThreadStart(DoTryProcess));
						InnerItemProcessThread.Start();

						//���� ���� �� ������������ item ��� �� ������� timeout
						DateTime startTime = DateTime.Now;
						while (InnerItemProcessThread.IsAlive) {
							TimeSpan interval = DateTime.Now - startTime;
							if (interval.TotalMilliseconds > Timeout) {
								//timed out
								EventHandler<QueueEventArgs> h2 = AfterProcessingTimedOut;
								if (h2 != null) AfterProcessingTimedOut(this, new QueueEventArgs(CurrentKey, Current));
								InnerItemProcessThread.Abort();
								throw new TimeOutException();
							}
						}
						delay = InnerDelay;
					} catch (Exception ex) {
						EventHandler<QueueErrorEventArgs> h1 = AfterError;
						QueueErrorEventArgs args1;
						if (h1 != null) {
							args1 = new QueueErrorEventArgs(CurrentKey, currentItem.Item, delay, ex, RetryCount);
							h1(this, args1);
						}

						delay = InnerDelay;
					}

					if (delay <= 0) {
						if (currentItem.State != QueueItemState.Paused) Dequeue(currentItem.Key);
					} else {
						RetryCount++;
						if (RetryCount >= myMaxRetry)
							InnerRetry = false;

						if (!InnerRetry)
							Delay(currentItem.Key, delay);
					}
				} while (InnerRetry);

				if (this.Count == 0) {
					//���� ������� ����� - ��������� ������� � ������� ���������(����������� ���������� ������)
					lock (InnerQueueEvent)
						InnerQueueEvent.Reset();
				}
			}
		}

		protected virtual void Dequeue(long ItemKey) {
			lock (this) {
				T item = Current;
				long itemKey = CurrentKey;
				Shift();

				OnAfterDequeue(ItemKey, item);

				foreach (ProcessingQueueItem<T> it in InnerItems) {
					if (it.Delay > 0) it.Delay--;
				}
			}
		}

		protected virtual void OnAfterDequeue(long ItemKey, T item) {
			EventHandler<QueueEventArgs> h = AfterDequeue;
			if (h != null)
				h(this, new QueueEventArgs(ItemKey, item));
		}

		protected virtual void OnAfterEnqueue(long ItemKey, T item) {
			EventHandler<QueueEventArgs> h = AfterEnqueue;
			if (h != null)
				h(this, new QueueEventArgs(ItemKey, item));
		}

		protected virtual void Delay(long ItemKey, int delay) {
			ProcessingQueueItem<T> item = GetItem(ItemKey);
			if (item == null) return;
			item.Delay = delay;
		}

		protected virtual void DoTryProcess() {
			ProcessingQueueItem<T> currentItem = CurrentItem;

			currentItem.State = QueueItemState.Processing;
			InnerDelay = TryProcess();

			if (currentItem.State == QueueItemState.Processing) currentItem.State = QueueItemState.Processed;
		}

		protected virtual int TryProcess() {
			return 0;
		}

		protected virtual ProcessingQueueItem<T> GetItem(long itemKey) {
			for (int i = 0; i < InnerItems.Count; ++i) if (InnerItems[i].Key == itemKey) return InnerItems[i];
			return null;
		}
		//.............................................................................
		#endregion
	}



	public enum QueueState {
		None,
		Working,
		Stopped
	}

	public enum QueueItemState {
		Waiting,
		Processing,
		Processed,
		Paused,
		NotExists,
		Error,
		Deffered
	}



	public class TimeOutException : Exception {
		public TimeOutException() : base() { }
		public TimeOutException(string message) : base(message) {}
	}



}