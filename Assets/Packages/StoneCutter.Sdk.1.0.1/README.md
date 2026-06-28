# StoneCutter

A .NET API and lightweight client SDK for managing stone records. The SDK is built on **.NET Standard 2.1**, making it compatible with **.NET Core 3.0+**, **.NET 5+**, **Xamarin**, and **Unity 2021.2+**.

---

## Table of Contents

- [API Endpoints](#api-endpoints)
  - [POST /api/stones](#post-apistones)
  - [GET /api/stones](#get-apistones)
- [SDK Reference](#sdk-reference)
  - [StoneCutterClient](#stonecutterclient)
  - [StoneData](#stonedata)
  - [StoneSizeType](#stonesizetype)
  - [PagedResponse\<T\>](#pagedresponset)
- [Unity Integration Guide](#unity-integration-guide)
  - [Prerequisites](#prerequisites)
  - [Adding Newtonsoft.Json](#adding-newtonsoftjson)
  - [Basic Usage in Unity](#basic-usage-in-unity)
  - [Coroutine Wrapper for UI](#coroutine-wrapper-for-ui)
  - [Infinite Scroll / Pagination](#infinite-scroll--pagination)
  - [Error Handling](#error-handling)
  - [Best Practices](#best-practices)

---

## API Endpoints

### `POST /api/stones`

Saves a new stone record to the database.

**Request Body** (`application/json`):

```json
{
  "name": "Emerald Shard",
  "stoneSize": 1,
  "stoneColor": "Green",
  "jadeCount": 5,
  "jsonContext": "{\"origin\":\"cave\"}"
}
```

| Field | Type | Required | Default | Description |
|---|---|---|---|---|
| `name` | `string` | **Yes** | — | Display name for the stone. |
| `stoneSize` | `int` | No | `0` (Small) | Size category: `0` = Small, `1` = Medium, `2` = Large. |
| `stoneColor` | `string` | No | `""` | Color of the stone. |
| `jadeCount` | `int` | No | `0` | Number of jade units attached. |
| `jsonContext` | `string` | No | `""` | Arbitrary JSON metadata string. |

**Response** (`201 Created`):

```json
{
  "id": 42,
  "name": "Emerald Shard",
  "stoneSize": 1,
  "stoneColor": "Green",
  "jadeCount": 5,
  "jsonContext": "{\"origin\":\"cave\"}",
  "createdAt": "2026-03-04T05:30:00Z"
}
```

---

### `GET /api/stones`

Retrieves stone records from the database with **pagination**, **sorting**, and **filtering**.

**Query Parameters:**

| Parameter | Type | Default | Description |
|---|---|---|---|
| `page` | `int` | `1` | 1-based page index. Clamped to a minimum of `1`. |
| `pageSize` | `int` | `10` | Items per page. Clamped to `1`–`50`. |
| `sortOrder` | `string` | `"desc"` | `"asc"` or `"desc"` by creation date, or `"random"` for random order. |
| `stoneSize` | `int?` | — | Filter by size: `0` = Small, `1` = Medium, `2` = Large. |
| `stoneColor` | `string?` | — | Filter by exact stone color. |
| `minJadeCount` | `int?` | — | Minimum jade count (inclusive). |
| `maxJadeCount` | `int?` | — | Maximum jade count (inclusive). |

**Example Request:**

```
GET /api/stones?page=1&pageSize=20&sortOrder=desc&stoneColor=Green&minJadeCount=3
```

**Response** (`200 OK`):

```json
{
  "items": [
    {
      "id": 42,
      "name": "Emerald Shard",
      "stoneSize": 1,
      "stoneColor": "Green",
      "jadeCount": 5,
      "jsonContext": "{\"origin\":\"cave\"}",
      "createdAt": "2026-03-04T05:30:00Z"
    }
  ],
  "page": 1,
  "pageSize": 20,
  "totalCount": 1,
  "totalPages": 1
}
```

---

## SDK Reference

### `StoneCutterClient`

The main entry point for all SDK operations. Implements `IDisposable`.

#### Constructors

| Signature | Description |
|---|---|
| `StoneCutterClient(string baseUrl)` | Creates a client with an internally managed `HttpClient`. |
| `StoneCutterClient(string baseUrl, HttpClient httpClient)` | Creates a client using an externally provided `HttpClient` (useful for testing or connection pooling). |

#### Methods

##### `SaveStone`

```csharp
Task<StoneData> SaveStone(
    string name,
    StoneSizeType stoneSize = StoneSizeType.Small,
    string stoneColor = "",
    int jadeCount = 0,
    string jsonContext = ""
)
```

Saves a new stone record via `POST /api/stones`.

| Parameter | Type | Default | Description |
|---|---|---|---|
| `name` | `string` | *(required)* | Display name for the stone. |
| `stoneSize` | `StoneSizeType` | `Small` | Size category of the stone. |
| `stoneColor` | `string` | `""` | Color of the stone. |
| `jadeCount` | `int` | `0` | Number of jade units attached. |
| `jsonContext` | `string` | `""` | Arbitrary JSON metadata string. |

**Returns:** `StoneData` — the persisted stone record including its server-assigned `Id` and `CreatedAt`.

---

##### `GetStones`

```csharp
Task<List<StoneData>> GetStones()
```

Convenience method that returns the **first 10 stones**, sorted by date descending. Internally calls `GET /api/stones?page=1&pageSize=10&sortOrder=desc`.

**Returns:** `List<StoneData>`

---

##### `GetStonesPaged`

```csharp
Task<PagedResponse<StoneData>> GetStonesPaged(
    int page = 1,
    int pageSize = 10,
    string sortOrder = "desc",
    StoneSizeType? stoneSize = null,
    string? stoneColor = null,
    int? minJadeCount = null,
    int? maxJadeCount = null
)
```

Full-featured paginated query with optional filters.

| Parameter | Type | Default | Description |
|---|---|---|---|
| `page` | `int` | `1` | 1-based page index. |
| `pageSize` | `int` | `10` | Items per page (server clamps to 1–50). |
| `sortOrder` | `string` | `"desc"` | `"asc"` or `"desc"` by creation date, or `"random"` for random order. |
| `stoneSize` | `StoneSizeType?` | `null` | Filter by stone size. |
| `stoneColor` | `string?` | `null` | Filter by exact stone color. |
| `minJadeCount` | `int?` | `null` | Minimum jade count (inclusive). |
| `maxJadeCount` | `int?` | `null` | Maximum jade count (inclusive). |

**Returns:** `PagedResponse<StoneData>` — includes `Items`, pagination metadata, and `HasNextPage`.

---

### `StoneData`

Represents a single stone record returned by the API.

| Property | Type | Description |
|---|---|---|
| `Id` | `int` | Unique identifier (server-assigned). |
| `Name` | `string` | Display name. |
| `StoneSize` | `StoneSizeType` | Size category. |
| `StoneColor` | `string` | Color of the stone. |
| `JadeCount` | `int` | Count of jade units. |
| `JsonContext` | `string` | Arbitrary JSON metadata. |
| `CreatedAt` | `DateTime` | Timestamp when the stone was created. |

---

### `StoneSizeType`

Enum representing stone size categories.

| Value | Integer | Description |
|---|---|---|
| `Small` | `0` | Small-sized stone. |
| `Medium` | `1` | Medium-sized stone. |
| `Large` | `2` | Large-sized stone. |

---

### `PagedResponse<T>`

Generic wrapper for paginated API results.

| Property | Type | Description |
|---|---|---|
| `Items` | `List<T>` | Records for the current page. |
| `Page` | `int` | Current page index (1-based). |
| `PageSize` | `int` | Requested items per page. |
| `TotalCount` | `int` | Total matching records across all pages. |
| `TotalPages` | `int` | Total number of pages. |
| `HasNextPage` | `bool` | `true` if more pages are available (computed). |

---

## Unity Integration Guide

### Prerequisites

| Requirement | Minimum Version |
|---|---|
| **Unity** | 2021.2+ (`.NET Standard 2.1` scripting backend) |
| **Scripting Backend** | Mono or IL2CPP |
| **Api Level** | .NET Standard 2.1 or .NET Framework (in Player Settings) |

### Adding Newtonsoft.Json

The SDK depends on **Newtonsoft.Json**. In Unity, install it via one of these methods:

**Option A — Unity Package Manager (Recommended):**

1. Open **Window → Package Manager**
2. Click **+** → **Add package by name**
3. Enter: `com.unity.nuget.newtonsoft-json`

**Option B — Manual DLL:**

1. Download `Newtonsoft.Json.dll` (v13.0.3, netstandard2.0 build) from [NuGet](https://www.nuget.org/packages/Newtonsoft.Json/13.0.3)
2. Place it in `Assets/Plugins/Newtonsoft/`

---

### Basic Usage in Unity

Create a MonoBehaviour to interact with the SDK using `async/await`:

```csharp
using UnityEngine;
using StoneCutter.Sdk;

public class StoneManager : MonoBehaviour
{
    private StoneCutterClient _client;

    [SerializeField] private string apiUrl = "https://your-api-url.com";

    private void Awake()
    {
        _client = new StoneCutterClient(apiUrl);
    }

    private void OnDestroy()
    {
        _client?.Dispose();
    }

    // Save a stone via POST /api/stones
    public async void SaveStone(string stoneName, StoneSizeType size, string color, int jade)
    {
        try
        {
            var stone = await _client.SaveStone(stoneName, size, color, jade);
            Debug.Log($"Stone saved! ID: {stone.Id}, Name: {stone.Name}, Color: {stone.StoneColor}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to save stone: {ex.Message}");
        }
    }

    // Get stones via GET /api/stones with pagination
    public async void LoadFirstPage()
    {
        try
        {
            var page = await _client.GetStonesPaged(page: 1, pageSize: 20); // randomly load stones from datbase

            foreach (var stone in page.Items)
                Debug.Log($"{stone.Name} — Size: {stone.StoneSize}, Color: {stone.StoneColor}, Jade: {stone.JadeCount}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to load stones: {ex.Message}");
        }
    }
}
```

> **Note:** `async void` is acceptable for Unity event handlers (button clicks, lifecycle methods). For internal logic, prefer `async Task` and propagate exceptions properly.

---

### Coroutine Wrapper for UI

If you prefer Unity coroutines over `async/await` (e.g., for progress indicators or frame-by-frame updates), wrap async calls like this:

```csharp
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using StoneCutter.Sdk;

public class StoneLoader : MonoBehaviour
{
    private StoneCutterClient _client;

    private void Awake()
    {
        _client = new StoneCutterClient("https://your-api-url.com");
    }

    private void OnDestroy()
    {
        _client?.Dispose();
    }

    public void StartLoading()
    {
        StartCoroutine(LoadStonesCoroutine());
    }

    private IEnumerator LoadStonesCoroutine()
    {
        var task = _client.GetStonesPaged(page: 1, pageSize: 10);

        // Wait for the async task to complete
        while (!task.IsCompleted)
            yield return null;

        if (task.IsFaulted)
        {
            Debug.LogError($"Error: {task.Exception?.InnerException?.Message}");
            yield break;
        }

        var result = task.Result;
        Debug.Log($"Loaded {result.Items.Count} of {result.TotalCount} stones");

        foreach (var stone in result.Items)
            Debug.Log($"  {stone.Name} — {stone.StoneColor}");
    }
}
```

---

### Infinite Scroll / Pagination

Use `GetStonesPaged` with the `HasNextPage` property to implement dynamic loading as the user scrolls:

```csharp
using System.Collections.Generic;
using UnityEngine;
using StoneCutter.Sdk;

public class InfiniteStoneScroller : MonoBehaviour
{
    private StoneCutterClient _client;
    private int _currentPage = 0;
    private bool _isLoading = false;
    private bool _hasMore = true;

    private readonly List<StoneData> _allStones = new List<StoneData>();

    [SerializeField] private int pageSize = 20;
    [SerializeField] private string apiUrl = "https://your-api-url.com";

    private void Awake()
    {
        _client = new StoneCutterClient(apiUrl);
    }

    private void OnDestroy()
    {
        _client?.Dispose();
    }

    /// <summary>
    /// Call this when the user scrolls near the bottom of the list.
    /// </summary>
    public async void LoadNextPage()
    {
        if (_isLoading || !_hasMore) return;

        _isLoading = true;
        _currentPage++;

        try
        {
            var page = await _client.GetStonesPaged(
                page: _currentPage,
                pageSize: pageSize,
                sortOrder: "desc"
            );

            _allStones.AddRange(page.Items);
            _hasMore = page.HasNextPage;

            Debug.Log($"Page {page.Page}/{page.TotalPages} loaded — " +
                      $"{_allStones.Count}/{page.TotalCount} total");

            // TODO: Update your ScrollView / ListView UI here
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to load page {_currentPage}: {ex.Message}");
            _currentPage--; // Revert so user can retry
        }
        finally
        {
            _isLoading = false;
        }
    }

    /// <summary>
    /// Call this with filter parameters to load a filtered + paginated view.
    /// </summary>
    public async void LoadFiltered(StoneSizeType? size, string stoneColor, int? minJade, int? maxJade)
    {
        _allStones.Clear();
        _currentPage = 0;
        _hasMore = true;

        _isLoading = true;
        _currentPage = 1;

        try
        {
            var page = await _client.GetStonesPaged(
                page: 1,
                pageSize: pageSize,
                stoneSize: size,
                stoneColor: stoneColor,
                minJadeCount: minJade,
                maxJadeCount: maxJade
            );

            _allStones.AddRange(page.Items);
            _hasMore = page.HasNextPage;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Filter query failed: {ex.Message}");
        }
        finally
        {
            _isLoading = false;
        }
    }
}
```

---

### Error Handling

The SDK throws exceptions in the following scenarios:

| Exception | When |
|---|---|
| `HttpRequestException` | Network failure or non-success HTTP status code. |
| `InvalidOperationException` | Response body could not be deserialized. |
| `TaskCanceledException` | Request timed out. |

**Recommended Unity pattern:**

```csharp
public async void SafeApiCall()
{
    try
    {
        var stones = await _client.GetStones();
        // Process stones...
    }
    catch (System.Net.Http.HttpRequestException ex)
    {
        // Show "No connection" UI
        Debug.LogWarning($"Network error: {ex.Message}");
    }
    catch (System.InvalidOperationException ex)
    {
        // Unexpected API response format
        Debug.LogError($"Data error: {ex.Message}");
    }
    catch (System.Exception ex)
    {
        // Catch-all
        Debug.LogError($"Unexpected error: {ex.Message}");
    }
}
```

---

### Best Practices

1. **Single client instance** — Create one `StoneCutterClient` in `Awake()` and dispose it in `OnDestroy()`. Avoid creating a new client per request.

2. **Don't block the main thread** — Always use `async/await` or coroutine wrappers. Never call `.Result` or `.Wait()` on the main thread — this will freeze your game.

3. **Handle scene transitions** — Dispose the client when changing scenes to avoid orphaned HTTP connections:
   ```csharp
   private void OnDestroy()
   {
       _client?.Dispose();
   }
   ```

4. **IL2CPP / AOT considerations** — If building with IL2CPP (iOS, WebGL), ensure `link.xml` preserves the SDK types from being stripped:
   ```xml
   <linker>
       <assembly fullname="StoneCutter.Sdk" preserve="all"/>
       <assembly fullname="Newtonsoft.Json" preserve="all"/>
   </linker>
   ```
   Place this file at `Assets/link.xml`.

5. **Thread safety** — `StoneCutterClient` is **not** thread-safe. If you need concurrent access from multiple threads, create separate client instances or synchronize access.

6. **Configure API URL** — Use a `[SerializeField]` field or `ScriptableObject` for the API URL so it can be changed per environment (dev/staging/prod) without recompiling.

