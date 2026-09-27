export interface User {
  id: string;
  fullName: string;
  firstName?: string | null;
  lastName?: string | null;
  profilePictureUrl?: string | null;
  phoneNumber?: string;
  email?: string | null;
  status?: string;
  phoneNumberVerified?: boolean;
  emailVerified?: boolean;
  role?: string;
}

export interface TokenResponse {
  accessToken: string;
  accessTokenExpiresAt?: string | number;
  refreshToken: string;
  refreshTokenExpiresAt?: string | number;
}

export interface AuthResponse {
  user: User;
  tokens: TokenResponse;
  token?: TokenResponse | string;
}

export interface LoginRequest {
  login?: string;
  email?: string;
  emailOrPhone?: string;
  password: string;
  deviceName?: string | null;
}

export interface RegisterRequest {
  fullName: string;
  phoneNumber: string;
  email?: string | null;
  password: string;
  deviceName?: string | null;
}

export interface SendOtpResponse {
  success: boolean;
}

export interface GoogleAuthRequest {
  idToken: string;
}
