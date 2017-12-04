using System;

namespace Verse
{
	public static class DataSerializeUtility
	{
		public static byte[] SerializeByte(int elements, Func<int, byte> reader)
		{
			byte[] array = new byte[elements];
			for (int i = 0; i < elements; i++)
			{
				array[i] = reader(i);
			}
			return array;
		}

		public static byte[] SerializeByte(byte[] data)
		{
			return data;
		}

		public static byte[] DeserializeByte(byte[] data)
		{
			return data;
		}

		public static void LoadByte(byte[] arr, int elements, Action<int, byte> writer)
		{
			if (arr == null || arr.Length == 0)
			{
				return;
			}
			for (int i = 0; i < elements; i++)
			{
				writer(i, arr[i]);
			}
		}

		public static byte[] SerializeUshort(int elements, Func<int, ushort> reader)
		{
			byte[] array = new byte[elements * 2];
			for (int i = 0; i < elements; i++)
			{
				ushort num = reader(i);
				array[i * 2] = (byte)(num >> 0 & 255);
				array[i * 2 + 1] = (byte)(num >> 8 & 255);
			}
			return array;
		}

		public static byte[] SerializeUshort(ushort[] data)
		{
			return DataSerializeUtility.SerializeUshort(data.Length, (int i) => data[i]);
		}

		public static ushort[] DeserializeUshort(byte[] data)
		{
			ushort[] result = new ushort[data.Length / 2];
			DataSerializeUtility.LoadUshort(data, result.Length, delegate(int i, ushort dat)
			{
				result[i] = dat;
			});
			return result;
		}

		public static void LoadUshort(byte[] arr, int elements, Action<int, ushort> writer)
		{
			if (arr == null || arr.Length == 0)
			{
				return;
			}
			for (int i = 0; i < elements; i++)
			{
				writer(i, (ushort)((int)arr[i * 2] << 0 | (int)arr[i * 2 + 1] << 8));
			}
		}

		public static byte[] SerializeInt(int elements, Func<int, int> reader)
		{
			byte[] array = new byte[elements * 4];
			for (int i = 0; i < elements; i++)
			{
				int num = reader(i);
				array[i * 4] = (byte)(num >> 0 & 255);
				array[i * 4 + 1] = (byte)(num >> 8 & 255);
				array[i * 4 + 2] = (byte)(num >> 16 & 255);
				array[i * 4 + 3] = (byte)(num >> 24 & 255);
			}
			return array;
		}

		public static byte[] SerializeInt(int[] data)
		{
			return DataSerializeUtility.SerializeInt(data.Length, (int i) => data[i]);
		}

		public static int[] DeserializeInt(byte[] data)
		{
			int[] result = new int[data.Length / 4];
			DataSerializeUtility.LoadInt(data, result.Length, delegate(int i, int dat)
			{
				result[i] = dat;
			});
			return result;
		}

		public static void LoadInt(byte[] arr, int elements, Action<int, int> writer)
		{
			if (arr == null || arr.Length == 0)
			{
				return;
			}
			for (int i = 0; i < elements; i++)
			{
				writer(i, (int)arr[i * 4] << 0 | (int)arr[i * 4 + 1] << 8 | (int)arr[i * 4 + 2] << 16 | (int)arr[i * 4 + 3] << 24);
			}
		}
	}
}
