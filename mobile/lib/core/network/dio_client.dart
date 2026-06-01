import 'dart:async';

import 'package:dio/dio.dart';
import 'package:flutter_dotenv/flutter_dotenv.dart';
import '../storage/secure_storage.dart';

class DioClient {
  late final Dio _dio;
  final SecureStorage _secureStorage;
  bool _isRefreshing = false;
  Completer<String?>? _refreshCompleter;

  DioClient(this._secureStorage) {
    final baseUrl = dotenv.env['BASE_URL'] ?? 'http://127.0.0.1:8000/api/v1';
    _dio = Dio(
      BaseOptions(
        baseUrl: baseUrl,
        connectTimeout: const Duration(seconds: 15),
        receiveTimeout: const Duration(seconds: 15),
        headers: {'Content-Type': 'application/json'},
      ),
    );
    _addInterceptors();
  }

  Dio get dio => _dio;

  void _addInterceptors() {
    _dio.interceptors.add(
      InterceptorsWrapper(
        onRequest: (options, handler) async {
          final token = await _secureStorage.getAccessToken();
          if (token != null) {
            options.headers['Authorization'] = 'Bearer $token';
          }
          handler.next(options);
        },
        onError: (error, handler) async {
          if (error.response?.statusCode == 401) {
            if (_isRefreshing) {
              try {
                final accessToken = await _refreshCompleter!.future;
                if (accessToken == null) {
                  handler.next(error);
                  return;
                }
                final retryResponse = await _retryRequest(error.requestOptions, accessToken);
                handler.resolve(retryResponse);
              } catch (_) {
                handler.next(error);
              }
              return;
            }

            _isRefreshing = true;
            _refreshCompleter = Completer<String?>();
            try {
              final accessToken = await _refreshAccessToken();
              _refreshCompleter?.complete(accessToken);
              if (accessToken == null) {
                handler.next(error);
                return;
              }
              final retryResponse = await _retryRequest(error.requestOptions, accessToken);
              handler.resolve(retryResponse);
            } catch (e) {
              _refreshCompleter?.completeError(e);
              await _secureStorage.clearTokens();
              handler.next(error);
            } finally {
              _isRefreshing = false;
              _refreshCompleter = null;
            }
          } else {
            handler.next(error);
          }
        },
      ),
    );
  }

  Future<String?> _refreshAccessToken() async {
    final refreshToken = await _secureStorage.getRefreshToken();
    if (refreshToken == null) {
      await _secureStorage.clearTokens();
      return null;
    }

    final response = await _dio.post(
      '/auth/refresh/?set_cookie=false',
      data: {'refresh_token': refreshToken},
      options: Options(
        headers: {'Authorization': null},
      ),
    );

    final newAccessToken = response.data['access'] as String?;
    final newRefreshToken = response.data['refresh'] as String?;
    if (newAccessToken == null) {
      await _secureStorage.clearTokens();
      return null;
    }

    await _secureStorage.saveAccessToken(newAccessToken);
    if (newRefreshToken != null) {
      await _secureStorage.saveRefreshToken(newRefreshToken);
    }

    return newAccessToken;
  }

  Future<Response<dynamic>> _retryRequest(
    RequestOptions requestOptions,
    String accessToken,
  ) async {
    requestOptions.headers['Authorization'] = 'Bearer $accessToken';
    return _dio.fetch(requestOptions);
  }
}
