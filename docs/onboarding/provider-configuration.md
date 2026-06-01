# Provider Configuration

Provider selection is backend-driven. Clients should discover active providers from backend system APIs instead of hard-coding a provider choice.

- Email: choose `EMAIL_PROVIDER` and optional `EMAIL_FALLBACK_PROVIDERS`.
- Push: choose `PUSH_PROVIDER` and configure Web Push, FCM, or OneSignal credentials.
- SMS: choose `SMS_PROVIDER` with Twilio or Vonage credentials.
- Analytics: choose `ANALYTICS_PROVIDER` with PostHog or Mixpanel credentials.
- Payments: enable the gateways needed for the downstream project.
- Public provider state can be inspected from `/api/v1/system/providers/` and `/api/v1/system/general-settings/`.

## How To Change Providers Safely

1. Set the provider name and credentials in backend config.
2. Confirm the adapter is registered in the backend service layer.
3. Verify `/api/v1/system/providers/` reflects the expected readiness state.
4. If the provider needs client SDK setup, expose only the minimum safe public values.
5. Test both the happy path and the fallback path if fallbacks are configured.
6. Keep sandbox and production credentials separate, and validate callback or webhook verification before go-live.
