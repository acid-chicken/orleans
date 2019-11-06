﻿using Newtonsoft.Json;
using System;
using System.Net;

namespace OneBoxDeployment.IntegrationTests
{
class IPAddressConverter: JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType.IsSubclassOf(typeof(IPAddress));
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        return IPAddress.Parse((string)reader.Value);
    }
}
}
