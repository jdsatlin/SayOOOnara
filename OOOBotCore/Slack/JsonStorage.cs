using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SayOOOnara
{
	public interface IStorage<T>
	{
		Task<List<T>> GetAll();

		Task SaveAll(List<T> objects);
	}
	public class JsonStorage<T> : IStorage<T>
	{
		private static string BaseDirectory
		{
			get
			{
				var directory = AppDomain.CurrentDomain.BaseDirectory;

				if (directory.Contains("bin"))
				{
					directory = Path.GetFullPath(Path.Combine(directory, @"..\..\..\"));
				}

				return directory;
			}
		}

		private static string PersistanceDirectory => Path.GetFullPath(Path.Combine(BaseDirectory, @"Persistance"));


		private static string JsonFile => Path.Combine(PersistanceDirectory, typeof(T) + "s.json");

		public JsonStorage()
		{
			if (!File.Exists(JsonFile))
			{
				
				File.Create(JsonFile).Dispose();
			}
		}

		public async Task<List<T>> GetAll()
		{
			string text;
			using (StreamReader reader = new StreamReader(File.OpenRead(JsonFile)))
			{
				text = await reader.ReadToEndAsync();
			}

			var objectsFromFile = new List<T>();
			if (text.Length > 0)
			{
				objectsFromFile = JsonConvert.DeserializeObject<List<T>>(text);
			}

			return objectsFromFile;

		}

		public async Task SaveAll(List<T> objects)
		{
			using (StreamWriter writer = new StreamWriter(File.Open(JsonFile, FileMode.Create)))
			{
				string text = JsonConvert.SerializeObject(objects);
				writer.WriteAsync(text);
			}

		}
	}
}