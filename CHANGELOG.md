# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [Unreleased]
- Functionality to redeem accrued overtime.
- Accrue overtime. 
- Export of custom timeframe.
- Export of current week only.

## 0.0.3 - 2018-11-30
### Added
- Added quick and dirty WPF UI.
### Changed
- Updated third party licenses.
### Removed
- Legacy project Yatt superseded by YattWpf
- Legacy project YattExport superseded by YattWpf

## 0.0.2 - 2018-10-21
### Added
- Excel Reporting Tool
- Basic export of all tracked time to Excel.
- Recognition of new day, starts a new timecard if the tool is running for multiple days.
### Changed
- Showing local time instead of UTC for clock in and clock out time.

## 0.0.1 - 2018-10-09
### Added
- Tracks clock in time when first launched.
- Tracks clock out time by saving the current time every 10 seconds.
- Tracks the total time the application has been running.
- Tracks the total time the computer was locked whilst the application was running.
- Tracks the total time the computer had no mouse or keyboard input (triggered after 10 seconds of inactivity) whilst the application was running.
- Tracks overtime where the total time exceeds the total number of working hours specified in the config file.
- Deducts specified lunch break time from the total time the application has been running and the total time the computer was locked whilst the application was running.
- This CHANGELOG file to serve as the standardized CHANGELOG of this project.
### Changed 
- ReadMe adding a few headers with placeholders with details to be added.
