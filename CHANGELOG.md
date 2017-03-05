# Change Log
I love building stuff, but I love people knowing what I've built too. So
here's a changelog for ConsulStrucutre!

The format is based on [Keep a Changelog](http://keepachangelog.com/)
and this project adheres to [Semantic Versioning](http://semver.org/).

## [0.0.3]
### Added
 - I think it's rude to respond to requests with an error, but I'm not
   a server, so I made requests backoff exponentially on error, in case
   the server mellows over time
 - ConsulStructure is so awesome you obviously don't want to stop it,
   but in those rare cases where you do, Cancellation on Stop is working.
 - It takes a village to get ConsulStructure out, well, a village of 1,
   and many build scripts, so said build scripts have been made reusable
 - For people that are curious, there are events for most things happening
 - For people that are crafters, there are factories for most things
   being used
 - For people that are an... I mean, picky, there are now coveralls.io,
   sonarqube and coverity scans for various types of builds, which
   helped uncover a bunch of edge case bugs in the test suite, which have
   been extinguished. The bugs, not the edge cases.

## [0.0.2]
### Added
 - ConsulStructure initial version, see the documentation on GitHub!

## [0.0.1]
### Added
 - Placeholder version