using System;
using System.Dynamic;
using System.IO;
using System.Threading;
using Melomans.Core.Message;
using Melomans.Core.Models;

namespace Melomans.Core.Network
{
	public abstract class NetworkTaskBase<TMessage> : INetworkTask<TMessage>
		where TMessage : class, IMessage
	{
		private Action<TMessage> _onFinally;
		private Action<Exception> _onCatch;
		private Action<TMessage> _onSuccess;
		private Action<TMessage> _onStart;
		private Action<TMessage> _onCancelled;
		private Action<ProgressInfo<TMessage>> _onReport;
		private CancellationTokenSource _cancellationTokenSource;
		private Func<TMessage, Stream> _getStream;

		protected bool IsCancellationRequested
		{
			get { return _cancellationTokenSource != null && _cancellationTokenSource.IsCancellationRequested; }
		}

		public NetworkTaskBase()
		{
			_cancellationTokenSource = new CancellationTokenSource();
		}

		protected void RaiseFinally(TMessage message)
		{
			if (_onFinally != null)
				_onFinally(message);
		}

		protected void RaiseCacth(Exception ex)
		{
			if (_onCatch != null)
				_onCatch(ex);
		}

		protected void RaiseSuccess(TMessage message)
		{
			if (_onSuccess != null)
				_onSuccess(message);
		}

		protected void RaiseStart(TMessage message)
		{
			if (_onStart != null)
				_onStart(message);
		}

		protected void RaiseCancelled(TMessage message)
		{
			if (_onCancelled != null)
				_onCancelled(message);
		}

		protected void RaiseReport(ProgressInfo<TMessage> info)
		{
			if (_onReport != null)
				_onReport(info);
		}

		protected Stream RaiseGetStream(TMessage message)
		{
			if (_getStream != null)
				return _getStream(message);
			return null;
		}

		public abstract Meloman For { get; }

		public virtual void Cancel()
		{
			if(_cancellationTokenSource != null)
				_cancellationTokenSource.Cancel();
		}

		public virtual INetworkTask<TMessage> OnFinally(Action<TMessage> onFinally)
		{
			_onFinally = onFinally;
			return this;
		}

		public virtual INetworkTask<TMessage> OnException(Action<Exception> onCatch)
		{
			_onCatch = onCatch;
			return this;
		}

		public virtual INetworkTask<TMessage> OnReport(Action<ProgressInfo<TMessage>> onReport)
		{
			_onReport = onReport;
			return this;
		}

		public virtual INetworkTask<TMessage> OnSuccess(Action<TMessage> onSuccess)
		{
			_onSuccess = onSuccess;
			return this;
		}

		public virtual INetworkTask<TMessage> GetStream(Func<TMessage, Stream> getStream)
		{
			_getStream = getStream;
			return this;
		}

		public void Run()
		{
			try
			{
				RaiseStart(Message);
				Run(_cancellationTokenSource.Token);
			}
			catch (OperationCanceledException)
			{
				RaiseCancelled(Message);
			}
			catch (Exception ex)
			{
				RaiseCacth(ex);
			}
			finally
			{
				if(_cancellationTokenSource != null)
					_cancellationTokenSource.Dispose();
				RaiseFinally(Message);
				_onCatch = null;
				_onCancelled = null;
				_onFinally = null;
				_onReport = null;
				_onStart = null;
				_onSuccess = null;
				_getStream = null;
			}
		}

		protected abstract TMessage Message { get; }

		protected abstract void Run(CancellationToken cancellationToken);

		public virtual INetworkTask<TMessage> OnStart(Action<TMessage> onStart)
		{
			_onStart = onStart;
			return this;
		}

		public virtual INetworkTask<TMessage> OnCancelled(Action<TMessage> onCancelled)
		{
			_onCancelled = onCancelled;
			return this;
		}
	}
}