﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Net.Http;

namespace Squidex.ClientLibrary.Utils
{
    internal static class HttpMethodEx
    {
        public static readonly HttpMethod Patch = new HttpMethod("PATCH");
    }
}
