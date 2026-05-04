#----------------------------------------------------------------
# Generated CMake target import file for configuration "Release".
#----------------------------------------------------------------

# Commands may need to know the format version.
set(CMAKE_IMPORT_FILE_VERSION 1)

# Import target "Cuttlefish::lib" for configuration "Release"
set_property(TARGET Cuttlefish::lib APPEND PROPERTY IMPORTED_CONFIGURATIONS RELEASE)
set_target_properties(Cuttlefish::lib PROPERTIES
  IMPORTED_LOCATION_RELEASE "${_IMPORT_PREFIX}/lib/libcuttlefish.so.2.10.0"
  IMPORTED_SONAME_RELEASE "libcuttlefish.so.2.10"
  )

list(APPEND _cmake_import_check_targets Cuttlefish::lib )
list(APPEND _cmake_import_check_files_for_Cuttlefish::lib "${_IMPORT_PREFIX}/lib/libcuttlefish.so.2.10.0" )

# Commands beyond this point should not need to know the version.
set(CMAKE_IMPORT_FILE_VERSION)
