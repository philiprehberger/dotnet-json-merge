# Changelog

## 0.2.0 (2026-03-28)

- Add array element matching by key via `ArrayMatchKey` option
- Add merge conflict callback via `OnConflict` delegate
- Add path-based selective merge via `PathFilter` option
- Add RFC 7396 JSON Merge Patch support via `MergePatch` method

## 0.1.2 (2026-03-24)

- Add unit tests
- Add test step to CI workflow

## 0.1.1 (2026-03-23)

- Convert API documentation from bullet lists to table format
- Fix license section formatting

## 0.1.0 (2026-03-22)

- Initial release
- Deep merge JSON documents using System.Text.Json
- Configurable array merge strategies (Replace, Concat, Union)
- Configurable null handling (Keep, Remove)
- MergeAll for merging multiple documents
