# Model simplified from Discogs API — Release & Master Release Structure

> **Endpoints**
> - Release: `GET https://api.discogs.com/releases/{release_id}`
> - Master Release: `GET https://api.discogs.com/masters/{master_id}`
> - Master Versions: `GET https://api.discogs.com/masters/{master_id}/versions`

---

## Concept: Release vs Master Release

| Concept | Description |
|---|---|
| **Master Release** | The canonical, format-agnostic entry for a recording. Acts as a parent grouping all physical/digital versions. |
| **Release** | A specific pressing or edition (e.g. US vinyl 1991, UK CD 1992). Always belongs to a master (if one exists). |

A master release has a `main_release` pointer — the "primary" version curated by the community.  
A release has a `master_id` + `master_url` pointing back to its parent master.

---

## Release Object

`GET /releases/{release_id}`

### Identity

| Field | Type | Description |
|---|---|---|
| `id` | integer | Unique release identifier |
| `title` | string | Release title |
| `status` | string | Community status: `Accepted`, `Draft`, `Deleted` |
| `uri` | string | Discogs website URL (relative path) |
| `resource_url` | string | API endpoint URL for this release |
| `data_quality` | string | Data completeness indicator (e.g. `Correct`, `Needs Vote`) |

### Release Info

| Field | Type | Description |
|---|---|---|
| `year` | integer | Year of this specific pressing |
| `released` | string | Full release date (`YYYY-MM-DD` or partial) |
| `released_formatted` | string | Human-readable formatted date |
| `country` | string | Country of release |
| `notes` | string | Free-text notes about this pressing |
| `thumb` | string | URL of thumbnail image (150×150) |

### Master Link

| Field | Type | Description |
|---|---|---|
| `master_id` | integer | ID of the parent master release (if any) |
| `master_url` | string | API URL of the parent master release |

### Classification

| Field | Type | Description |
|---|---|---|
| `genres` | `string[]` | Genre tags (e.g. `["Rock", "Electronic"]`) |
| `styles` | `string[]` | Style sub-tags (e.g. `["Ambient", "Post-Rock"]`) |

---

### `artists[]` — Main Artists

```
artists[]:
  id            integer   Artist ID
  name          string    Artist name (canonical)
  anv           string    Artist Name Variation (as credited on this release)
  join          string    Join string between artists (e.g. "&", ",")
  role          string    Role if credited differently
  tracks        string    Tracks where this artist appears
  resource_url  string    API URL for the artist
```


---

### `labels[]` — Labels & Catalog Numbers

```
labels[]:
  id                integer
  name              string    Label name
  catno             string    Catalog number
  entity_type       string    Numeric type code ("1" = Label)
  entity_type_name  string    Human-readable type ("Label", "Series", "Company")
  resource_url      string
```


### `formats[]` — Physical / Digital Formats

```
formats[]:
  name          string    Medium name (e.g. "Vinyl", "CD", "Cassette", "File")
  qty           string    Quantity of this medium in the release
  text          string    Optional freeform description
  descriptions  string[]  Format qualifiers (e.g. ["LP", "Album", "Stereo", "180g"])
```

---

### `tracklist[]` — Track Listing

```
tracklist[]:
  position      string    Track position (e.g. "A1", "B2", "1", "2-3")
  type_         string    Track type: "track", "index", "heading"
  title         string    Track title
  duration      string    Duration formatted as "M:SS"

  artists[]:              Per-track artist override (same shape as release artists[])
    id            integer
    name          string
    anv           string
    join          string
    role          string
    resource_url  string

  extraartists[]:         Per-track additional credits (same shape)
    id            integer
    name          string
    anv           string
    role          string
    tracks        string
    resource_url  string

  sub_tracks[]:           Sub-tracks (for medleys or index tracks)
    position      string
    type_         string
    title         string
    duration      string
```

---

### `identifiers[]` — Barcodes & Matrix Numbers

```
identifiers[]:
  type          string    e.g. "Barcode", "Matrix / Runout", "ASIN", "ISRC"
  value         string    The identifier value
  description   string    Optional freeform note
```

---

### `images[]` — Cover Art & Media Scans

```
images[]:
  type          string    "primary" | "secondary"
  uri           string    Full-size image URL (requires auth)
  uri150        string    Thumbnail URL 150×150 (requires auth)
  width         integer   Image width in px
  height        integer   Image height in px
  resource_url  string
```

---

### `videos[]` — Linked Videos

```
videos[]:
  uri           string    YouTube or external video URL
  title         string    Video title
  description   string    Video description
  duration      integer   Duration in seconds
  embed         boolean   Whether embedding is allowed
```

---

### `community{}` — Community Data

```
community:
  status        string    "Accepted" | "Draft" | "Needs Vote" | "Deleted"

  rating:
    count       integer   Number of ratings submitted
    average     float     Average rating (0.00–5.00)

  want          integer   Number of users who want this release
  have          integer   Number of users who own this release

  contributors[]:         Users who submitted data for this release
    username    string
    resource_url string

  submitter:              Original submitter
    username    string
    resource_url string

  data_quality  string    Inherited or overridden data quality
```


---

### Timestamps

| Field | Type | Description |
|---|---|---|
| `date_added` | string (ISO 8601) | When the release was added to the database |
| `date_changed` | string (ISO 8601) | Last modification date |

---

## Master Release Object

`GET /masters/{master_id}`

Shares most fields with a Release, with these differences:

| Field | Type | Description |
|---|---|---|
| `id` | integer | Master release ID (distinct namespace from release IDs) |
| `title` | string | Canonical title |
| `year` | integer | Year of the earliest known release |
| `main_release` | integer | ID of the designated primary Release |
| `main_release_url` | string | API URL of the primary Release |
| `most_recent_release` | integer | ID of the most recently added Release version |
| `most_recent_release_url` | string | API URL |
| `versions_url` | string | API URL to list all Release versions |
| `num_for_sale` | integer | Aggregate marketplace listings across all versions |
| `lowest_price` | float | Lowest price across all versions |

> **Note:** Master releases do **not** have `labels[]`, `formats[]`, `country`, or `catno` at the master level.
> These fields only exist on individual Release versions.

---

## Master Versions List

`GET /masters/{master_id}/versions`

Supports pagination. Sortable by: `released`, `title`, `format`, `label`, `catno`, `country`.

```
pagination:
  page          integer
  pages         integer
  per_page      integer
  items         integer
  urls:
    first       string
    prev        string
    next        string
    last        string

versions[]:
  id            integer   Release ID of this version
  title         string
  released      string    Release year or full date
  country       string
  format        string    Condensed format string (e.g. "Vinyl, LP, Album")
  major_formats string[]  Top-level format names
  label         string    Primary label name
  catno         string    Catalog number
  thumb         string    Thumbnail URL
  status        string
  resource_url  string
```

---

## Key Design Notes

**Master ≠ Release ID space.** `masters/26631` and `releases/26631` are entirely different objects.

**`anv` (Artist Name Variation).** When an artist is credited under a different name on a release, `name` holds the canonical name and `anv` holds the credited name. Your UI should display `anv` when non-empty.

**`type_` on tracklist.** Tracks with `type_: "heading"` are section dividers (e.g. "Side A"), not playable tracks. `type_: "index"` groups sub-tracks.

**Images require authentication.** `uri` and `uri150` return 401 without a valid Discogs token or OAuth credentials.

**`formats[].descriptions[]`** carries rich metadata: pressing weight (`180g`), speed (`45 RPM`), edition tags (`Limited Edition`, `Promo`, `Reissue`, `Repress`), and packaging (`Gatefold`, `Digipak`).

**`data_quality`** values in practice: `Correct`, `Needs Vote`, `Complete And Correct`, `Needs Minor Changes`, `Entirely Incorrect`.
