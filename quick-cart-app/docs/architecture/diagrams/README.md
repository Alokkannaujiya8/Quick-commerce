# QuickCart Frontend — Architecture Diagrams

This document contains visual Mermaid architectural diagrams modeling the runtime flows, layout hierarchy, state management, and authentication mechanics of the QuickCart frontend application.

---

## 1. Application Layout & Component Hierarchy

```mermaid
flowchart TD
    AppRoot["App Root Shell (src/app/app.ts)"]
    RouterOutlet["Root Router Outlet (<router-outlet />)"]
    
    subgraph CommerceLayout["MainLayoutComponent (/)]
        Header["HeaderComponent (Logo, Delivery Badge, Search, ThemeToggle, UserMenu, CartTrigger)"]
        ChildOutlet["Child Router Outlet"]
        CartDrawer["CartDrawerComponent (Slide-Out Drawer)"]
        Footer["FooterComponent (Guarantees, Links)"]
        
        ProductList["ProductListComponent (Home /)"]
        Checkout["CheckoutComponent (/checkout)"]
        OrderSuccess["OrderSuccessComponent (/order-success/:id)"]
    end
    
    subgraph AuthViews["Dedicated Authentication Views"]
        Login["LoginComponent (/login)"]
        OtpVerify["OtpVerifyComponent (/login/verify)"]
        PasswordLogin["PasswordLoginComponent (/login/password)"]
        Register["RegisterComponent (/register)"]
    end

    AppRoot --> RouterOutlet
    RouterOutlet --> CommerceLayout
    RouterOutlet --> AuthViews
    
    Header -.-> CartDrawer
    ChildOutlet --> ProductList
    ChildOutlet --> Checkout
    ChildOutlet --> OrderSuccess
```

---

## 2. Multi-Channel Customer Authentication Flow

```mermaid
sequenceDiagram
    autonumber
    actor Customer as Customer
    participant UI as LoginComponent / OtpVerify / GIS
    participant Auth as AuthService
    participant Storage as localStorage
    participant API as QuickCart.Api (/api/auth)

    alt OTP Authentication
        Customer->>UI: Enter 10-digit mobile number
        UI->>Auth: sendOtp(phoneNumber)
        Auth->>API: POST /api/auth/otp/send
        API-->>Auth: 200 OK { success: true }
        UI->>Customer: Navigate to /login/verify
        Customer->>UI: Enter 6-digit OTP code
        UI->>Auth: verifyOtp(phoneNumber, code)
        Auth->>API: POST /api/auth/otp/verify
        API-->>Auth: 200 OK AuthResponse (Tokens + User)
    else Google Sign-In
        Customer->>UI: Click "Continue with Google"
        UI->>UI: GIS popup renders & authenticates
        UI->>Auth: googleSignIn(idToken)
        Auth->>API: POST /api/auth/google
        API-->>Auth: 200 OK AuthResponse (Tokens + User)
    else Email/Password
        Customer->>UI: Submit email & password
        UI->>Auth: login(credentials)
        Auth->>API: POST /api/auth/login
        API-->>Auth: 200 OK AuthResponse (Tokens + User)
    end

    Auth->>Storage: setItem('qc_access_token', token)
    Auth->>Storage: setItem('qc_refresh_token', refreshToken)
    Auth->>Storage: setItem('qc_user_profile', userJson)
    Auth->>Auth: currentUser.set(user)
    UI->>Customer: Navigate to Home (/)
```

---

## 3. Checkout & Payment Intent Settlement Flow

```mermaid
sequenceDiagram
    autonumber
    actor Customer as Customer
    participant Checkout as CheckoutComponent
    participant Cart as CartService
    participant Orders as OrderService
    participant Payments as PaymentService
    participant API as QuickCart.Api Host

    Customer->>Checkout: Fill Delivery Address & Select Payment (UPI/Card/COD)
    Customer->>Checkout: Click "Place Order"
    Checkout->>Orders: createOrder(request)
    Orders->>API: POST /api/orders/checkout
    API-->>Orders: 201 Created (BackendOrderDto)
    Orders-->>Checkout: OrderResponse (id, orderNumber)

    Checkout->>Payments: createPaymentIntent(orderId, method, idempotencyKey)
    Payments->>API: POST /api/payments/intents
    API-->>Payments: 200 OK (PaymentIntentResponse)
    Payments-->>Checkout: intent (paymentId, providerOrderId)

    alt Payment Method is COD
        Checkout->>Orders: updateOrderPaymentState(orderId, 'Pending')
    else Online Payment (UPI / Card)
        Checkout->>Payments: verifyPayment(paymentId, providerOrderId, sig)
        Payments->>API: POST /api/payments/verify
        API-->>Payments: 200 OK (PaymentDto status: Captured)
        Checkout->>Orders: updateOrderPaymentState(orderId, 'Captured', transactionId)
    end

    Checkout->>Cart: clearCart()
    Checkout->>Customer: Navigate to /order-success/:id
```

---

## 4. Signal-Driven Reactive Cart Engine

```mermaid
flowchart LR
    subgraph UI_Actions["User Interaction"]
        AddBtn["Click 'Add to Cart'"]
        QtyBtn["Click '+' or '-'"]
        RemoveBtn["Click 'Remove'"]
    end

    subgraph CartStore["CartService (Signals Store)"]
        items["items: WritableSignal<CartItem[]>"]
        subtotal["subtotal: ComputedSignal"]
        deliveryFee["deliveryFee: ComputedSignal"]
        totalAmount["totalAmount: ComputedSignal"]
        totalItems["totalItems: ComputedSignal"]
    end

    subgraph Sync["Storage Sync"]
        LocalStorage["localStorage ('qc_cart_items')"]
    end

    subgraph HeaderView["Header & Drawer View"]
        Badge["Header Badge (Count & Amount)"]
        Drawer["Slide-Out Cart Drawer"]
    end

    AddBtn --> items
    QtyBtn --> items
    RemoveBtn --> items

    items --> subtotal
    items --> totalItems
    subtotal --> deliveryFee
    subtotal --> totalAmount
    deliveryFee --> totalAmount

    items -.-> LocalStorage
    totalItems --> Badge
    subtotal --> Badge
    items --> Drawer
```

---

## 5. HTTP Interceptor & Token Refresh Rotation

```mermaid
flowchart TD
    Req["HTTP Request Dispatched"]
    CheckToken{"Has Access Token in Storage?"}
    AddBearer["Set Header 'Authorization: Bearer <token>'"]
    Send["Send via Angular HttpClient"]
    
    ResponseCheck{"Response Status"}
    Success["200/201/204 Success -> Emit Response"]
    Err401{"Is Status 401 & Not Auth Endpoint?"}
    PassErr["Emit Error to Calling Service"]
    
    CheckRefresh{"Has Refresh Token in Storage?"}
    Refresh["POST /api/auth/refresh"]
    RefreshResult{"Refresh Succeeded?"}
    SaveNew["Store New Access Token"]
    Retry["Retry Original Request with New Bearer Token"]
    Purge["clearSession() & Redirect to /login"]

    Req --> CheckToken
    CheckToken -- Yes --> AddBearer --> Send
    CheckToken -- No --> Send
    
    Send --> ResponseCheck
    ResponseCheck -- 2xx --> Success
    ResponseCheck -- Other Error --> Err401
    
    Err401 -- Yes --> CheckRefresh
    Err401 -- No --> PassErr
    
    CheckRefresh -- Yes --> Refresh
    CheckRefresh -- No --> PassErr
    
    Refresh --> RefreshResult
    RefreshResult -- Yes --> SaveNew --> Retry --> Send
    RefreshResult -- No --> Purge --> PassErr
```
