﻿#region -- License Terms --
//
// MessagePack for CLI
//
// Copyright (C) 2010-2012 FUJIWARA, Yusuke
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
//
#endregion -- License Terms --

#if !SILVERLIGHT
using System;
using System.Collections.Specialized;
using System.Runtime.Serialization;

namespace MsgPack.Serialization.DefaultSerializers
{
	internal sealed class System_Collections_Specialized_NameValueCollectionMessagePackSerializer : MessagePackSerializer<NameValueCollection>
	{
		protected internal sealed override void PackToCore( Packer packer, NameValueCollection objectTree )
		{
			if ( objectTree == null )
			{
				packer.PackNull();
				return;
			}

			packer.PackMapHeader( objectTree.Count );
			foreach ( string key in objectTree )
			{
				if ( key == null )
				{
					ThrowNullKeyIsNotSupported();
				}

				var values = objectTree.GetValues( key );
				if ( values == null )
				{
					// Ignore
					continue;
				}

				packer.PackString( key );
				packer.PackArrayHeader( values.Length );
				foreach ( var value in values )
				{
					packer.PackString( value );
				}
			}
		}

		private static void ThrowNullKeyIsNotSupported()
		{
			throw new NotSupportedException( "null key is not supported." );
		}

		protected internal sealed override NameValueCollection UnpackFromCore( Unpacker unpacker )
		{
			var result = new NameValueCollection( checked( ( int )unpacker.ItemsCount ) );

			while ( unpacker.Read() )
			{
				var key = unpacker.Data.Value.DeserializeAsString();
				if ( !unpacker.Read() )
				{
					SerializationExceptions.ThrowUnexpectedEndOfStream();
				}

				if ( !unpacker.IsArrayHeader )
				{
					ThrowInvalidNameValueCollectionValue();
				}

				using ( var valuesUnpacker = unpacker.ReadSubtree() )
				{
					while ( valuesUnpacker.Read() )
					{
						result.Add( key, unpacker.Data.Value.DeserializeAsString() );
					}
				}
			}

			return result;
		}

		private static void ThrowInvalidNameValueCollectionValue()
		{
			throw new SerializationException( "Invalid NameValueCollection value." );
		}
	}
}
#endif
