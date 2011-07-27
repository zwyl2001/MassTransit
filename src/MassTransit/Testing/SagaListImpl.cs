// Copyright 2007-2011 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace MassTransit.Testing
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading;
	using Magnum.Extensions;
	using Saga;

	public class SagaListImpl<T> :
		SagaList<T>,
		IDisposable
		where T : class, ISaga
	{
		readonly HashSet<SagaInstance<T>> _messages;
		readonly AutoResetEvent _received;
		TimeSpan _timeout = 8.Seconds();

		public SagaListImpl()
		{
			_messages = new HashSet<SagaInstance<T>>(new SagaEqualityComparer());
			_received = new AutoResetEvent(false);
		}

		public void Dispose()
		{
			using (_received)
			{
			}
		}

		public IEnumerator<SagaInstance<T>> GetEnumerator()
		{
			lock (_messages)
				return _messages.ToList().GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public bool Any(Func<T, bool> filter)
		{
			bool any;

			Func<SagaInstance<T>, bool> predicate = x => filter(x.Saga);

			lock (_messages)
				any = _messages.Any(predicate);

			while (any == false)
			{
				if (_received.WaitOne(_timeout, true) == false)
					return false;

				lock (_messages)
				{
					any = _messages.Any(predicate);
				}
			}

			return true;
		}

		public bool Any()
		{
			bool any;
			lock (_messages)
				any = _messages.Any();

			while (any == false)
			{
				if (_received.WaitOne(_timeout, true) == false)
					return false;

				lock (_messages)
					any = _messages.Any();
			}

			return true;
		}

		public void Add(SagaInstance<T> message)
		{
			lock (_messages)
			{
				if (_messages.Add(message))
					_received.Set();
			}
		}

		class SagaEqualityComparer :
			IEqualityComparer<SagaInstance<T>>
		{
			public bool Equals(SagaInstance<T> x, SagaInstance<T> y)
			{
				return Equals(x.Saga.CorrelationId, y.Saga.CorrelationId);
			}

			public int GetHashCode(SagaInstance<T> message)
			{
				return message.Saga.CorrelationId.GetHashCode();
			}
		}
	}
}