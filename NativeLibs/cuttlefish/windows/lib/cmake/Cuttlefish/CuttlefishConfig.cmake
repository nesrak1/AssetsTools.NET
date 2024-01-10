include(${CMAKE_CURRENT_LIST_DIR}/cuttlefish_lib-targets.cmake)
set(Cuttlefish_LIBRARIES Cuttlefish::lib)
get_target_property(Cuttlefish_INCLUDE_DIRS Cuttlefish::lib INTERFACE_INCLUDE_DIRECTORIES)
include(${CMAKE_CURRENT_LIST_DIR}/cuttlefish-targets.cmake)
