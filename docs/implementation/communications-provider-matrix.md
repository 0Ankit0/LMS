# Communications Provider Matrix

| Channel | Active Providers | Notes |
|---|---|---|
| Email | SMTP, Resend, SES | Environment-driven selection with fallback list |
| Push | Web Push, FCM, OneSignal | Device registry stores provider-specific identifiers and preference responses should reflect active registered devices |
| SMS | Twilio, Vonage | Shared delivery contract through communications service |
| Analytics | PostHog, Mixpanel | Backend and clients choose provider by config |
| Payments | Khalti, eSewa, Stripe, PayPal | Existing finance registry remains available |
