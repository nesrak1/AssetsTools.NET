/*
 * Copyright 2016-2023 Aaron Barany
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#pragma once

/**
 * @file
 * @brief Configuration macros for the project.
 */

#if defined(_WIN32)
#	define CUTTLEFISH_WINDOWS 1
#elif defined(linux)
#	define CUTTLEFISH_LINUX 1
#elif defined(__APPLE__)
#	define CUTTLEFISH_APPLE
#endif

#if defined(_MSC_VER)
#	define CUTTLEFISH_MSC 1
#elif defined(__clang__)
#	define CUTTLEFISH_CLANG 1
#elif defined(__GNUC__)
#	define CUTTLEFISH_GCC 1
#else
#error Unknown compiler.
#endif

/**
 * @brief Define for whether the platform is Windows.
 */
#ifndef CUTTLEFISH_WINDOWS
#	define CUTTLEFISH_WINDOWS 0
#endif

/**
 * @brief Define for whether the platform is Linux.
 */
#ifndef CUTTLEFISH_LINUX
#	define CUTTLEFISH_LINUX 0
#endif

/**
 * @brief Define for whether the platform is Apple.
 */
#ifndef CUTTLEFISH_APPLE
#	define CUTTLEFISH_APPLE 0
#endif

/**
 * @brief Define for whether the compler is Microsoft's C compiler.
 */
#ifndef CUTTLEFISH_MSC
#	define CUTTLEFISH_MSC 0
#endif

/**
 * @brief Define for whether the compiler is LLVM clang.
 */
#ifndef CUTTLEFISH_CLANG
#	define CUTTLEFISH_CLANG 0
#endif

/**
 * @def CUTTLEFISH_GCC
 * @brief Define for whether the compiler is GCC.
 */
#ifndef CUTTLEFISH_GCC
#	define CUTTLEFISH_GCC 0
#endif

/**
 * @brief Macro defined to whether or not the system is 64-bit.
 */
#if defined(__LP64__) || defined(_WIN64) || defined(__x86_64__) || defined(__ppc64__) || defined(__arm64__)
#define CUTTLEFISH_64BIT 1
#else
#define CUTTLEFISH_64BIT 0
#endif

/**
 * @brief Macro defined to whether or not the system is 64-bit x86.
 */
#if defined(__x86_64__) || defined(_M_AMD64)
#define CUTTLEFISH_X86_64 1
#else
#define CUTTLEFISH_X86_64 0
#endif

/**
 * @brief Macro defined to whether or not the system is 32-bit x86.
 */
#if defined(__i386__) || defined(_M_IX86)
#define CUTTLEFISH_X86_32 1
#else
#define CUTTLEFISH_X86_32 0
#endif

/**
 * @brief Macro defined to whether or not the system is 64-bit ARM.
 */
#if defined(__arm64__) || defined(__aarch64__)
#define CUTTLEFISH_ARM_64 1
#else
#define CUTTLEFISH_ARM_64 0
#endif

/**
 * @brief Macro defined to whether or not the system is 32-bit ARM.
 */
#if defined(__arm__) || defined(_M_ARM)
#define CUTTLEFISH_ARM_32 1
#else
#define CUTTLEFISH_ARM_32 0
#endif

/**
 * @brief Define for whether or not this is a debug build.
 */
#ifdef NDEBUG
#define CUTTLEFISH_DEBUG 0
#else
#define CUTTLEFISH_DEBUG 1
#endif

/**
 * @brief Macro for an unused variable.
 *
 * This can be used to work around compiler warnings.
 * @param x The unused variable.
 */
#define CUTTLEFISH_UNUSED(x) (void)(&x)

#if CUTTLEFISH_MSC
#pragma warning(disable: 4251) // 'x' needs to have dll-interface to be used by clients of class 'y'
#endif
