## [4.0.5](https://github.com/MirageNet/SteamyFaceNG/compare/v4.0.4...v4.0.5) (2021-09-01)


### Bug Fixes

* sends dont cause endpoint changes ([32fc25f](https://github.com/MirageNet/SteamyFaceNG/commit/32fc25f8568ac9a6e0e09b5633629f28cd68db6c))

## [4.0.4](https://github.com/MirageNet/SteamyFaceNG/compare/v4.0.3...v4.0.4) (2021-07-26)


### Performance Improvements

* use class for SteamEndPoint to prevent boxing ([88b95bc](https://github.com/MirageNet/SteamyFaceNG/commit/88b95bc805b86d91e2d9a2115886b36159062459))

## [4.0.3](https://github.com/MirageNet/SteamyFaceNG/compare/v4.0.2...v4.0.3) (2021-07-21)


### Bug Fixes

* switched Facepunch.Posix to Linux ([48b02af](https://github.com/MirageNet/SteamyFaceNG/commit/48b02af3d029d2ed793a42369e7b4976fc61abef))

## [4.0.2](https://github.com/MirageNet/SteamyFaceNG/compare/v4.0.1...v4.0.2) (2021-07-21)


### Bug Fixes

* update Facepunch libs for Linux ([230624c](https://github.com/MirageNet/SteamyFaceNG/commit/230624cd20570066098cad280a52151327129d7c))

## [4.0.1](https://github.com/MirageNet/SteamyFaceNG/compare/v4.0.0...v4.0.1) (2021-07-20)


### Bug Fixes

* build on Linux no longer fails ([ca0822c](https://github.com/MirageNet/SteamyFaceNG/commit/ca0822cdd0adbbb4fdb54d47b6e593e81736b314))

# [4.0.0](https://github.com/MirageNet/SteamyFaceNG/compare/v3.0.0...v4.0.0) (2021-07-09)


### Features

* Mirage Socket support ([#8](https://github.com/MirageNet/SteamyFaceNG/issues/8)) ([18bf024](https://github.com/MirageNet/SteamyFaceNG/commit/18bf02459bc3dbbad489c584042e5f1597abb7c9))


### BREAKING CHANGES

* switched to new ISocket API, requires Mirage 96+

* docs: Updated README

* feat: Added options to control Steam init and running callbacks

* feat: Support latest Mirage
* SteamEndPoint implements IEndPoint

* use struct instead of class

* Update deps

* fix: Prevent peer from trying to send data on closed connections

* doc: readme

# [3.0.0](https://github.com/MirageNet/SteamyFaceNG/compare/v2.0.0...v3.0.0) (2021-03-16)


### Bug Fixes

* using Mirage.Logging namespace for LogFactory ([8b0d2c8](https://github.com/MirageNet/SteamyFaceNG/commit/8b0d2c864c1ebe27030bf4d21d9a7a4d72367af6))


### BREAKING CHANGES

* Requires Mirage 80.0.0+

# [2.0.0](https://github.com/MirageNet/SteamyFaceNG/compare/v1.0.4...v2.0.0) (2021-03-11)


### Code Refactoring

* use Send instead of SendAsync ([261c841](https://github.com/MirageNet/SteamyFaceNG/commit/261c841e7e3f22f692f229233e087b4c82ce969f))


### BREAKING CHANGES

* Compatibility with Mirage 74.0.0+

## [1.0.4](https://github.com/MirageNet/SteamyFaceNG/compare/v1.0.3...v1.0.4) (2021-02-20)


### Bug Fixes

* add meta for CHANGELOG ([ace4090](https://github.com/MirageNet/SteamyFaceNG/commit/ace4090b70b073669716f37b08afd5810f41d337))
* readme ([f5c59b3](https://github.com/MirageNet/SteamyFaceNG/commit/f5c59b36e658b5dec7fd3065819d5e290dc29ae0))

## [1.0.3](https://github.com/MirageNet/SteamyFaceNG/compare/v1.0.2...v1.0.3) (2021-02-19)


### Bug Fixes

* Fix Mirage compatibility ([6de7f76](https://github.com/MirageNet/SteamyFaceNG/commit/6de7f76e5e9868432d8a21baaab547847aa4b7f2))
* readme ([1a46fe5](https://github.com/MirageNet/SteamyFaceNG/commit/1a46fe508bbff177e3465b638a18690cd3f810e3))
* release package ([e7dabc7](https://github.com/MirageNet/SteamyFaceNG/commit/e7dabc789a0e7233fbf55803a2a0db5e01f1303f))

## [1.0.2](https://github.com/MirageNet/SteamyFaceNG/compare/v1.0.1...v1.0.2) (2021-02-19)


### Bug Fixes

* version for mirage. ([6c6d777](https://github.com/MirageNet/SteamyFaceNG/commit/6c6d7771a5ac592b562c2422fc239baca5eccd7d))

## [1.0.1](https://github.com/MirageNet/SteamyFaceNG/compare/v1.0.0...v1.0.1) (2021-02-18)


### Bug Fixes

* Folder ([1106608](https://github.com/MirageNet/SteamyFaceNG/commit/1106608eb5221aa0452a0a5165c4b9d2cc7c55d9))
* Folder name. ([7270318](https://github.com/MirageNet/SteamyFaceNG/commit/727031835155474afed89b0a92f3d495613b8f39))

# 1.0.0 (2021-01-15)


### Bug Fixes

* Fixed folder structure ([89f4365](https://github.com/MirrorNG/SteamyFaceNG/commit/89f43652e8330ea5050e8a56dee9f0bf43bdff96))
* folder structure ([70cbda5](https://github.com/MirrorNG/SteamyFaceNG/commit/70cbda50b5578e12d38c9737de093aeae05acc57))
* For missing invokes for transports]. ([ab38284](https://github.com/MirrorNG/SteamyFaceNG/commit/ab38284d932f60f8f4e22cdad2ce07b9a9adcff3))
* Messed up for CI/CD ([7ea3a18](https://github.com/MirrorNG/SteamyFaceNG/commit/7ea3a186ca0f2c1ceeaddbbf46e53824652f5c97))
* Missing release file for building. ([2678bdf](https://github.com/MirrorNG/SteamyFaceNG/commit/2678bdf07ff24ff01fce967101c7726afd96979c))
* Work to setup CI/CD ([10489bb](https://github.com/MirrorNG/SteamyFaceNG/commit/10489bb134739ec273286780b24d9fe5431ddf60))
