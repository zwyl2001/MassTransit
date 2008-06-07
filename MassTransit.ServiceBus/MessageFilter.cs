/// Copyright 2007-2008 The Apache Software Foundation.
/// 
/// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
/// this file except in compliance with the License. You may obtain a copy of the 
/// License at 
/// 
///   http://www.apache.org/licenses/LICENSE-2.0 
/// 
/// Unless required by applicable law or agreed to in writing, software distributed 
/// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
/// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
/// specific language governing permissions and limitations under the License.
namespace MassTransit.ServiceBus
{
	using System;

	public class MessageFilter<T> : Consumes<T>.All where T : class
	{
		private readonly Consumes<T>.All _consumer;
		private readonly Predicate<T> _filterFunction;

		public MessageFilter(Predicate<T> filterFunction, Consumes<T>.All consumer)
		{
			_filterFunction = filterFunction;
			_consumer = consumer;
		}

		public void Consume(T message)
		{
			if (_filterFunction(message))
			{
				_consumer.Consume(message);
			}
		}
	}
}