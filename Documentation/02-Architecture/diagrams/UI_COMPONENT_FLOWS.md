# Nalam360 Enterprise Platform - UI Component Flow Diagrams

This document contains interaction diagrams for Blazor UI components showing validation, data binding, and permission flows.

**Format:** Mermaid  
**Date:** November 18, 2025

---

## Table of Contents

1. [Form Validation Flow](#1-form-validation-flow)
2. [Data Grid CRUD Operations](#2-data-grid-crud-operations)
3. [File Upload with Preview](#3-file-upload-with-preview)
4. [Permission-Based Rendering](#4-permission-based-rendering)
5. [Theme Switching](#5-theme-switching)
6. [Modal Dialog Flow](#6-modal-dialog-flow)
7. [Autocomplete Search](#7-autocomplete-search)
8. [Notification System](#8-notification-system)
9. [Wizard Component](#9-wizard-component)
10. [Chart Data Refresh](#10-chart-data-refresh)

---

## 1. Form Validation Flow

**Description:** Multi-stage validation in N360Form with schema-driven rules and async validation.

```mermaid
sequenceDiagram
    participant User
    participant Form as N360Form
    participant Validator as ValidationRules
    participant AsyncVal as Async Validator
    participant API
    participant Toast as N360Toast

    User->>Form: Fill form fields
    
    loop On each field change
        User->>Form: Input change event
        Form->>Validator: ValidateField(fieldName, value)
        
        Validator->>Validator: Check required rule
        Validator->>Validator: Check pattern rule
        Validator->>Validator: Check custom rule
        
        alt Validation passed
            Validator-->>Form: ValidationResult (valid)
            Form->>Form: Clear error message
            Form->>Form: Update field style (valid)
        else Validation failed
            Validator-->>Form: ValidationResult (errors)
            Form->>Form: Show error message
            Form->>Form: Update field style (error)
        end
    end
    
    User->>Form: Click Submit
    
    Form->>Form: Prevent default submit
    Form->>Validator: ValidateAll()
    
    Validator->>Validator: Run all sync validations
    
    alt Has async validators
        Form->>AsyncVal: ValidateAsync()
        AsyncVal->>API: Check unique email
        API-->>AsyncVal: Email exists: false
        AsyncVal-->>Form: Validation passed
    end
    
    alt Form invalid
        Form->>Toast: ShowError("Please fix errors")
        Form->>Form: Focus first error field
        Form-->>User: Show validation errors
    else Form valid
        Form->>Form: Disable submit button
        Form->>API: POST /api/data (form data)
        
        alt API success
            API-->>Form: 201 Created
            Form->>Toast: ShowSuccess("Saved!")
            Form->>Form: Reset form
            Form->>Form: OnSubmitSuccess callback
            Form-->>User: Success feedback
        else API error
            API-->>Form: 400 Bad Request
            Form->>Toast: ShowError("Save failed")
            Form->>Form: Enable submit button
            Form->>Form: Show server errors
            Form-->>User: Error feedback
        end
    end
```

---

## 2. Data Grid CRUD Operations

**Description:** N360DataGrid with server-side operations, inline editing, and optimistic updates.

```mermaid
sequenceDiagram
    participant User
    participant Grid as N360DataGrid
    participant Toolbar as Grid Toolbar
    participant API
    participant Cache as State Cache
    participant Modal as N360Modal
    participant Toast

    Note over User,Toast: Initial Load
    
    Grid->>API: GET /api/orders?page=1&size=20
    API-->>Grid: OrderPage (items, total, page)
    Grid->>Grid: Render rows
    Grid->>Cache: Store current page
    
    Note over User,Toast: Create Operation
    
    User->>Toolbar: Click "Add" button
    Toolbar->>Modal: Show create dialog
    User->>Modal: Fill form & submit
    
    Modal->>API: POST /api/orders
    
    alt Create success
        API-->>Modal: 201 Created (new order)
        Modal->>Grid: Refresh data
        Grid->>API: GET /api/orders?page=1
        API-->>Grid: Updated page
        Grid->>Grid: Render with new row
        Modal->>Toast: ShowSuccess("Created")
        Modal->>Modal: Close dialog
    else Create failed
        API-->>Modal: 400 Bad Request
        Modal->>Toast: ShowError(error)
    end
    
    Note over User,Toast: Update Operation
    
    User->>Grid: Click edit button (row 3)
    Grid->>Grid: Enter edit mode
    Grid->>Grid: Show inline editors
    
    User->>Grid: Modify fields
    User->>Grid: Click save
    
    Grid->>Grid: Validate changes
    
    alt Validation passed
        Grid->>Cache: Store original data
        Grid->>Grid: Optimistic update (show new values)
        
        Grid->>API: PUT /api/orders/123
        
        alt Update success
            API-->>Grid: 200 OK (updated order)
            Grid->>Grid: Exit edit mode
            Grid->>Toast: ShowSuccess("Updated")
            Grid->>Cache: Clear backup
        else Update failed
            API-->>Grid: 400 Bad Request
            Grid->>Cache: Restore original data
            Grid->>Grid: Revert to original values
            Grid->>Toast: ShowError(error)
        end
    else Validation failed
        Grid->>Toast: ShowError("Fix errors")
    end
    
    Note over User,Toast: Delete Operation
    
    User->>Grid: Click delete button (row 5)
    Grid->>Modal: Show confirmation
    Modal-->>User: "Delete this order?"
    
    User->>Modal: Confirm delete
    
    Modal->>Grid: OnConfirm callback
    Grid->>Cache: Store deleted row
    Grid->>Grid: Remove row (optimistic)
    
    Grid->>API: DELETE /api/orders/456
    
    alt Delete success
        API-->>Grid: 204 No Content
        Grid->>Toast: ShowSuccess("Deleted")
        Grid->>Cache: Clear backup
    else Delete failed
        API-->>Grid: 400 Bad Request
        Grid->>Cache: Restore deleted row
        Grid->>Grid: Re-add row
        Grid->>Toast: ShowError("Delete failed")
    end
    
    Note over User,Toast: Pagination
    
    User->>Grid: Click page 2
    Grid->>API: GET /api/orders?page=2&size=20
    API-->>Grid: OrderPage (page 2)
    Grid->>Grid: Render new page
    Grid->>Cache: Store current page
```

---

## 3. File Upload with Preview

**Description:** N360FileUpload with drag-drop, preview generation, and progress tracking.

```mermaid
sequenceDiagram
    participant User
    participant Upload as N360FileUpload
    participant FileReader as File Reader
    participant Preview as Preview Generator
    participant API
    participant Progress as Progress Bar

    User->>Upload: Drag files over zone
    Upload->>Upload: Highlight drop zone
    
    User->>Upload: Drop 3 files
    
    Upload->>Upload: Validate file types
    Upload->>Upload: Validate file sizes
    
    loop For each file
        Upload->>Upload: Check: .jpg, .png, .pdf allowed?
        Upload->>Upload: Check: < 10MB?
        
        alt File invalid
            Upload->>Upload: Show error badge
            Note right of Upload: Red X icon, error message
        else File valid
            Upload->>Upload: Add to upload queue
            Upload->>Upload: Show file card
            
            alt Is image file
                Upload->>FileReader: Read as data URL
                FileReader-->>Upload: Data URL
                Upload->>Preview: Generate thumbnail
                Preview->>Preview: Create canvas
                Preview->>Preview: Scale image (150x150)
                Preview-->>Upload: Thumbnail blob
                Upload->>Upload: Display thumbnail
            else Is document
                Upload->>Upload: Show document icon
            end
        end
    end
    
    User->>Upload: Click "Upload All"
    
    loop For each valid file
        Upload->>Upload: Create FormData
        Upload->>API: POST /api/files/upload (multipart)
        
        API->>Upload: Upload progress events
        
        loop Progress updates
            API->>Progress: Progress (45%, 1.2MB/2.7MB)
            Progress->>Progress: Update bar width
            Progress->>Progress: Update label
        end
        
        alt Upload success
            API-->>Upload: 200 OK {url, id, size}
            Upload->>Upload: Mark file as uploaded
            Upload->>Upload: Show success icon
            Upload->>Upload: Add download link
            Upload->>Upload: OnUploadComplete(file)
        else Upload failed
            API-->>Upload: 500 Error
            Upload->>Upload: Mark file as failed
            Upload->>Upload: Show retry button
            Upload->>Upload: Show error message
        end
    end
    
    Note over User,Upload: Remove File
    
    User->>Upload: Click remove (file 2)
    Upload->>Upload: Show confirmation
    
    alt File already uploaded
        User->>Upload: Confirm remove
        Upload->>API: DELETE /api/files/123
        API-->>Upload: 204 No Content
        Upload->>Upload: Remove from list
    else File not uploaded
        Upload->>Upload: Remove from queue
    end
```

---

## 4. Permission-Based Rendering

**Description:** N360Button with RBAC permission checks and conditional rendering.

```mermaid
sequenceDiagram
    participant App as Blazor App
    participant Button as N360Button
    participant PermService as Permission Service
    participant Cache as Permission Cache
    participant AuthContext as Auth Context
    participant API as Permission API

    App->>Button: Render button
    Note right of App: <N360Button RequiredPermission="orders.delete" />
    
    Button->>Button: OnInitialized()
    Button->>AuthContext: Get current user
    AuthContext-->>Button: UserId, Roles
    
    alt No RequiredPermission set
        Button->>Button: Render normally (visible)
        Button-->>App: Button rendered
    else RequiredPermission set
        Button->>PermService: HasPermissionAsync("orders.delete")
        
        PermService->>Cache: GetCachedPermissions(userId)
        
        alt Cache hit
            Cache-->>PermService: Permission[] (cached)
            PermService->>PermService: Check if "orders.delete" in list
        else Cache miss
            Cache-->>PermService: null
            
            PermService->>API: GET /api/permissions/user/{userId}
            API->>API: Get user roles
            API->>API: Get role permissions
            API->>API: Get user-specific permissions
            API->>API: Apply denials
            API-->>PermService: Permission[]
            
            PermService->>Cache: SetAsync(permissions, 5min TTL)
            PermService->>PermService: Check if "orders.delete" in list
        end
        
        alt Has permission
            PermService-->>Button: true
            
            Button->>Button: Set IsAuthorized = true
            Button->>Button: Render button (enabled)
            Button->>Button: Add onclick handler
            Button-->>App: Button visible & clickable
        else No permission
            PermService-->>Button: false
            
            Button->>Button: Set IsAuthorized = false
            
            alt HideIfNoPermission = true
                Button->>Button: Don't render
                Button-->>App: Nothing rendered
            else HideIfNoPermission = false
                Button->>Button: Render disabled
                Button->>Button: Add tooltip "No permission"
                Button->>Button: Add disabled style
                Button-->>App: Button visible but disabled
            end
        end
    end
    
    Note over App,API: User clicks button (if authorized)
    
    User->>Button: Click event
    
    alt IsAuthorized = true
        Button->>Button: Fire OnClick callback
        Button->>App: Execute handler
        
        alt EnableAudit = true
            Button->>API: POST /api/audit/log
            Note right of Button: Resource: "Order #123"<br/>Action: "Delete"<br/>UserId: current user
        end
    else IsAuthorized = false
        Button->>Button: Prevent click
        Note right of Button: No action taken
    end
```

---

## 5. Theme Switching

**Description:** ThemeService managing dynamic theme changes with CSS variable updates.

```mermaid
sequenceDiagram
    participant User
    participant Toggle as N360ThemeToggle
    participant Service as ThemeService
    participant Storage as Local Storage
    participant CSS as CSS Variables
    participant Components as All Components

    Note over User,Components: Application Load
    
    Service->>Storage: GetItem("user-theme")
    
    alt Theme saved
        Storage-->>Service: "dark"
        Service->>Service: SetCurrentTheme("dark")
    else No theme saved
        Storage-->>Service: null
        Service->>Service: Detect system preference
        Service->>Service: SetCurrentTheme(systemTheme)
    end
    
    Service->>CSS: Apply theme CSS variables
    CSS->>CSS: Set --primary-color
    CSS->>CSS: Set --background-color
    CSS->>CSS: Set --text-color
    CSS->>CSS: Set --border-color
    Note right of CSS: 30+ CSS variables updated
    
    Service->>Components: Notify theme changed
    Components->>Components: Re-render with new theme
    
    Note over User,Components: User Changes Theme
    
    User->>Toggle: Click theme toggle
    Toggle->>Service: ToggleThemeAsync()
    
    Service->>Service: Get current theme
    Note right of Service: Current: "dark"
    
    Service->>Service: Calculate next theme
    Note right of Service: Next: "light"
    
    Service->>Service: Load theme configuration
    Service->>Service: Validate theme exists
    
    alt Theme valid
        Service->>CSS: Start transition
        CSS->>CSS: Add transition class
        Note right of CSS: .theme-transition { <br/>  transition: all 0.3s <br/>}
        
        Service->>CSS: Update CSS variables
        CSS->>CSS: --primary-color: #007bff
        CSS->>CSS: --background-color: #ffffff
        CSS->>CSS: --text-color: #212529
        
        Service->>Service: Update current theme
        Service->>Storage: SetItem("user-theme", "light")
        
        Service->>Components: NotifyThemeChanged("light")
        
        loop For each component
            Components->>Components: OnThemeChanged callback
            Components->>Components: Update state
            Components->>Components: Re-render
        end
        
        Service->>CSS: Wait 300ms
        Service->>CSS: Remove transition class
        
        Service-->>Toggle: Theme changed successfully
        Toggle-->>User: Visual feedback
    else Theme invalid
        Service-->>Toggle: Error
        Toggle->>Toggle: Show error toast
    end
    
    Note over User,Components: Custom Theme
    
    User->>Service: SetCustomThemeAsync(customConfig)
    Service->>Service: Validate configuration
    Service->>Service: Merge with base theme
    Service->>CSS: Apply custom variables
    Service->>Storage: Save custom config
    Service->>Components: Notify theme changed
```

---

## 6. Modal Dialog Flow

**Description:** N360Modal with async confirmation and result handling.

```mermaid
sequenceDiagram
    participant Page as Parent Page
    participant Button as Trigger Button
    participant Modal as N360Modal
    participant Content as Modal Content
    participant Overlay as Backdrop Overlay
    participant Service as Modal Service

    Button->>Page: User clicks "Delete"
    Page->>Service: ShowAsync(confirmOptions)
    
    Service->>Modal: Show(options)
    Modal->>Modal: Set IsVisible = true
    Modal->>Modal: Create TaskCompletionSource
    
    par Render modal components
        Modal->>Overlay: Show backdrop
        Overlay->>Overlay: Fade in (opacity: 0 → 0.5)
    and
        Modal->>Content: Render modal content
        Content->>Content: Slide in (transform: translateY(-50px))
    end
    
    Modal->>Modal: Set focus on modal
    Modal->>Modal: Trap keyboard focus
    Modal->>Modal: Listen for Escape key
    
    Content-->>User: Show dialog
    Note right of Content: Title: "Confirm Delete"<br/>Message: "Delete order #123?"<br/>Buttons: Cancel, Confirm
    
    alt User clicks Confirm
        User->>Content: Click confirm button
        Content->>Modal: OnConfirm()
        
        alt HasAsyncConfirm = true
            Modal->>Modal: Disable buttons
            Modal->>Modal: Show loading spinner
            Modal->>Page: Execute async confirm action
            Page->>Page: await DeleteOrderAsync()
            
            alt Async action succeeds
                Page-->>Modal: Success
                Modal->>Modal: TaskCompletionSource.SetResult(true)
                Modal->>Modal: StartClose()
            else Async action fails
                Page-->>Modal: Exception
                Modal->>Modal: Show error in modal
                Modal->>Modal: Enable buttons
                Note right of Modal: Keep modal open
            end
        else No async confirm
            Modal->>Modal: TaskCompletionSource.SetResult(true)
            Modal->>Modal: StartClose()
        end
        
    else User clicks Cancel
        User->>Content: Click cancel button
        Content->>Modal: OnCancel()
        Modal->>Modal: TaskCompletionSource.SetResult(false)
        Modal->>Modal: StartClose()
        
    else User presses Escape
        User->>Modal: Press ESC key
        Modal->>Modal: OnEscapeKey()
        
        alt AllowEscapeClose = true
            Modal->>Modal: TaskCompletionSource.SetResult(false)
            Modal->>Modal: StartClose()
        else AllowEscapeClose = false
            Note right of Modal: Ignore escape
        end
        
    else User clicks backdrop
        User->>Overlay: Click outside modal
        Overlay->>Modal: OnBackdropClick()
        
        alt AllowBackdropClose = true
            Modal->>Modal: TaskCompletionSource.SetResult(false)
            Modal->>Modal: StartClose()
        else AllowBackdropClose = false
            Note right of Modal: Ignore click
        end
    end
    
    Note over Page,Service: Close Animation
    
    Modal->>Content: Fade out
    Modal->>Overlay: Fade out
    Modal->>Modal: Wait for animation (200ms)
    Modal->>Modal: Set IsVisible = false
    Modal->>Modal: Release focus trap
    Modal->>Modal: Return focus to trigger
    
    Modal-->>Service: ModalResult
    Service-->>Page: await result
    
    alt Result = true (confirmed)
        Page->>Page: Execute post-confirm logic
    else Result = false (cancelled)
        Page->>Page: Execute cancel logic
    end
```

---

## 7. Autocomplete Search

**Description:** N360Autocomplete with debounced server search and keyboard navigation.

```mermaid
sequenceDiagram
    participant User
    participant Input as N360Autocomplete
    participant Debouncer as Debounce Timer
    participant API
    participant Cache as Search Cache
    participant Dropdown as Results Dropdown

    User->>Input: Focus input
    Input->>Input: Show search icon
    Input->>Dropdown: Prepare dropdown (hidden)
    
    User->>Input: Type "john"
    
    loop For each keystroke
        Input->>Input: Update input value
        Input->>Debouncer: Reset timer
        Debouncer->>Debouncer: Start 300ms countdown
        
        alt User types again (within 300ms)
            User->>Input: Type next char
            Input->>Debouncer: Cancel previous timer
            Debouncer->>Debouncer: Restart 300ms countdown
        end
    end
    
    Debouncer->>Debouncer: 300ms elapsed (no more typing)
    Debouncer->>Input: Trigger search
    
    Input->>Input: Get search term: "john"
    Input->>Input: Validate min length (3 chars)
    
    alt Term too short
        Input->>Dropdown: Hide
        Input-->>User: Show "Type at least 3 chars"
    else Term valid
        Input->>Cache: CheckCache("john")
        
        alt Cache hit
            Cache-->>Input: Cached results
            Input->>Dropdown: Show results
            Dropdown-->>User: Display 5 items
        else Cache miss
            Input->>Input: Show loading spinner
            Input->>API: GET /api/users/search?q=john
            
            API->>API: Execute search query
            API-->>Input: UserSearchResult[] (5 items)
            
            Input->>Cache: Store results (TTL: 5min)
            Input->>Input: Hide loading spinner
            
            alt Has results
                Input->>Dropdown: Populate with items
                Dropdown->>Dropdown: Highlight first item
                Dropdown->>Dropdown: Show dropdown
                Dropdown-->>User: Display results
            else No results
                Input->>Dropdown: Show "No results"
                Dropdown-->>User: Empty state
            end
        end
    end
    
    Note over User,Dropdown: Keyboard Navigation
    
    User->>Input: Press ArrowDown
    Input->>Dropdown: Move highlight down
    Dropdown->>Dropdown: Unhighlight current
    Dropdown->>Dropdown: Highlight next
    
    User->>Input: Press ArrowUp
    Input->>Dropdown: Move highlight up
    
    User->>Input: Press Enter
    Input->>Dropdown: Get highlighted item
    Dropdown-->>Input: User {id: 123, name: "John Doe"}
    
    Input->>Input: Set SelectedValue = item
    Input->>Input: Update input text = "John Doe"
    Input->>Dropdown: Hide dropdown
    Input->>Input: OnSelectionChanged(item)
    Input-->>User: Selection confirmed
    
    Note over User,Dropdown: Mouse Selection
    
    User->>Dropdown: Hover item 3
    Dropdown->>Dropdown: Highlight item 3
    
    User->>Dropdown: Click item 3
    Dropdown->>Input: OnItemClick(item)
    Input->>Input: Set SelectedValue = item
    Input->>Input: Update input text
    Input->>Dropdown: Hide dropdown
    Input->>Input: OnSelectionChanged(item)
    
    Note over User,Dropdown: Clear Selection
    
    User->>Input: Click clear button (X)
    Input->>Input: Clear SelectedValue
    Input->>Input: Clear input text
    Input->>Input: OnSelectionChanged(null)
    Input->>Dropdown: Hide dropdown
```

---

## 8. Notification System

**Description:** N360Toast notification queue with auto-dismiss and stacking.

```mermaid
sequenceDiagram
    participant App as Application
    participant Service as Toast Service
    participant Queue as Toast Queue
    participant Container as Toast Container
    participant Toast as Individual Toast
    participant Timer as Auto-Dismiss Timer

    Note over App,Timer: Show Success Toast
    
    App->>Service: ShowSuccess("Order created!")
    Service->>Service: Create toast object
    Note right of Service: id: guid<br/>type: success<br/>message: "Order created!"<br/>duration: 3000ms
    
    Service->>Queue: Enqueue(toast)
    Queue->>Queue: Check max toasts (limit: 5)
    
    alt Queue full
        Queue->>Queue: Dequeue oldest toast
        Queue->>Container: Remove oldest
    end
    
    Queue->>Container: Add toast to DOM
    Container->>Toast: Render toast
    
    Toast->>Toast: Slide in from right
    Note right of Toast: transform: translateX(100%) → 0
    
    Toast->>Timer: Start countdown (3000ms)
    
    Timer->>Timer: Tick every 100ms
    Timer->>Toast: Update progress bar
    
    alt User hovers toast
        Toast->>Timer: Pause countdown
        Toast->>Toast: Show close button
    end
    
    alt User moves mouse away
        Toast->>Timer: Resume countdown
        Toast->>Toast: Hide close button
    end
    
    Timer->>Timer: Countdown complete
    Timer->>Service: OnDismiss(toastId)
    Service->>Queue: Remove(toastId)
    Service->>Toast: StartRemove()
    
    Toast->>Toast: Slide out to right
    Toast->>Toast: Wait 300ms (animation)
    Toast->>Container: Remove from DOM
    
    Note over App,Timer: Show Multiple Toasts
    
    App->>Service: ShowError("Save failed")
    Service->>Queue: Enqueue (position 2)
    
    App->>Service: ShowInfo("Processing...")
    Service->>Queue: Enqueue (position 3)
    
    App->>Service: ShowWarning("Low stock")
    Service->>Queue: Enqueue (position 4)
    
    Container->>Container: Stack toasts vertically
    Note right of Container: Toast 1: top: 10px<br/>Toast 2: top: 80px<br/>Toast 3: top: 150px<br/>Toast 4: top: 220px
    
    loop For each toast
        Container->>Toast: Animate in with delay
        Note right of Toast: Stagger animation<br/>by 50ms each
    end
    
    Note over App,Timer: Manual Dismiss
    
    User->>Toast: Click close button (X)
    Toast->>Service: OnManualDismiss(toastId)
    Service->>Timer: Cancel countdown
    Service->>Queue: Remove(toastId)
    Service->>Toast: StartRemove()
    Toast->>Toast: Fade out & slide out
    Toast->>Container: Remove from DOM
    
    Container->>Container: Recalculate positions
    loop For remaining toasts
        Container->>Toast: Animate to new position
        Note right of Toast: Smooth transition
    end
```

---

## 9. Wizard Component

**Description:** N360Wizard multi-step form with validation and navigation.

```mermaid
sequenceDiagram
    participant User
    participant Wizard as N360Wizard
    participant Step1 as Step 1 Content
    participant Step2 as Step 2 Content
    participant Step3 as Step 3 Content
    participant Validator as Validation
    participant Nav as Step Navigation
    participant API

    Wizard->>Wizard: Initialize wizard
    Wizard->>Wizard: Set current step = 1
    Wizard->>Nav: Render step indicators
    Note right of Nav: Step 1 (active)<br/>Step 2 (pending)<br/>Step 3 (pending)
    
    Wizard->>Step1: Render step 1 content
    Step1-->>User: Show personal info form
    
    User->>Step1: Fill fields
    User->>Nav: Click "Next"
    
    Nav->>Wizard: OnNext()
    Wizard->>Validator: ValidateStep(1)
    
    Validator->>Step1: Get form values
    Step1-->>Validator: FormData
    Validator->>Validator: Run validation rules
    
    alt Step 1 invalid
        Validator-->>Wizard: ValidationResult (errors)
        Wizard->>Step1: Show validation errors
        Wizard-->>User: Remain on step 1
    else Step 1 valid
        Validator-->>Wizard: ValidationResult (valid)
        
        Wizard->>Wizard: Save step 1 data
        Wizard->>Wizard: Set current step = 2
        
        Wizard->>Step1: Hide step 1
        Wizard->>Step2: Show step 2
        
        Wizard->>Nav: Update indicators
        Note right of Nav: Step 1 (complete ✓)<br/>Step 2 (active)<br/>Step 3 (pending)
        
        Step2-->>User: Show address form
    end
    
    User->>Step2: Fill address
    User->>Nav: Click "Next"
    
    Nav->>Wizard: OnNext()
    Wizard->>Validator: ValidateStep(2)
    
    alt Step 2 invalid
        Validator-->>Wizard: Errors
        Wizard->>Step2: Show errors
    else Step 2 valid
        Wizard->>Wizard: Save step 2 data
        Wizard->>Wizard: Set current step = 3
        
        Wizard->>Step2: Hide step 2
        Wizard->>Step3: Show step 3
        
        Wizard->>Nav: Update indicators
        Note right of Nav: Step 1 (complete ✓)<br/>Step 2 (complete ✓)<br/>Step 3 (active)
        
        Step3->>Step3: Load summary data
        Step3-->>User: Show summary & review
    end
    
    alt User wants to go back
        User->>Nav: Click "Previous"
        Nav->>Wizard: OnPrevious()
        
        Wizard->>Wizard: Set current step = 2
        Wizard->>Step3: Hide step 3
        Wizard->>Step2: Show step 2 (with saved data)
        Wizard->>Nav: Update indicators
    end
    
    Note over User,API: Final Submit
    
    User->>Step3: Review summary
    User->>Nav: Click "Submit"
    
    Nav->>Wizard: OnSubmit()
    Wizard->>Validator: ValidateAll()
    
    Validator->>Validator: Validate step 1
    Validator->>Validator: Validate step 2
    Validator->>Validator: Validate step 3
    
    alt Any step invalid
        Validator-->>Wizard: Errors with step numbers
        Wizard->>Wizard: Navigate to first invalid step
        Wizard->>Step1: Show errors
        Wizard-->>User: Fix errors message
    else All steps valid
        Wizard->>Wizard: Disable submit button
        Wizard->>Wizard: Show loading spinner
        
        Wizard->>Wizard: Combine all step data
        Wizard->>API: POST /api/registration (complete data)
        
        alt API success
            API-->>Wizard: 201 Created
            Wizard->>Wizard: Set complete = true
            Wizard->>Wizard: Show success step
            Wizard->>Nav: Update indicators (all complete)
            Wizard-->>User: Success message & redirect
        else API error
            API-->>Wizard: 400 Bad Request
            Wizard->>Wizard: Enable submit button
            Wizard->>Wizard: Hide loading spinner
            Wizard->>Step3: Show API errors
            Wizard-->>User: Error message
        end
    end
```

---

## 10. Chart Data Refresh

**Description:** N360Chart with real-time data updates and smooth transitions.

```mermaid
sequenceDiagram
    participant User
    participant Chart as N360Chart
    participant Timer as Refresh Timer
    participant API
    participant Syncfusion as SfChart
    participant Animation as Chart Animation

    Chart->>Chart: OnInitialized()
    Chart->>API: GET /api/metrics/revenue?period=today
    API-->>Chart: ChartData (initial)
    
    Chart->>Syncfusion: Initialize chart
    Syncfusion->>Syncfusion: Render axes
    Syncfusion->>Syncfusion: Render series
    Syncfusion->>Animation: Animate in
    Animation-->>User: Chart appears
    
    alt AutoRefresh enabled
        Chart->>Timer: Start timer (RefreshInterval: 30s)
    end
    
    Note over User,Animation: Auto Refresh
    
    Timer->>Timer: 30 seconds elapsed
    Timer->>Chart: OnTimerTick()
    
    Chart->>API: GET /api/metrics/revenue?period=today
    Note right of Chart: Silent refresh (no loading)
    
    API-->>Chart: ChartData (updated)
    
    Chart->>Chart: Compare with current data
    
    alt Data changed
        Chart->>Chart: Prepare transition
        Chart->>Syncfusion: UpdateDataSource(newData)
        
        Syncfusion->>Animation: Start transition
        Animation->>Animation: Fade out old points
        Animation->>Animation: Morph to new points
        Animation->>Animation: Fade in new points
        Note right of Animation: Duration: 500ms<br/>Easing: ease-in-out
        
        Animation-->>User: Smooth data update
        
        Chart->>Chart: OnDataUpdated callback
    else Data unchanged
        Note right of Chart: Skip update
    end
    
    Timer->>Timer: Restart timer
    
    Note over User,Animation: Manual Refresh
    
    User->>Chart: Click refresh button
    Chart->>Chart: Set IsLoading = true
    Chart->>Chart: Show loading overlay
    
    Chart->>API: GET /api/metrics/revenue?period=today
    API-->>Chart: ChartData
    
    Chart->>Syncfusion: UpdateDataSource(newData)
    Chart->>Animation: Animate update
    
    Chart->>Chart: Set IsLoading = false
    Chart->>Chart: Hide loading overlay
    Chart-->>User: Updated chart
    
    Note over User,Animation: Change Time Period
    
    User->>Chart: Select "Last 7 days"
    Chart->>Chart: OnPeriodChanged("last7days")
    Chart->>Chart: Show loading
    
    Chart->>Timer: Pause auto-refresh
    Chart->>API: GET /api/metrics/revenue?period=last7days
    API-->>Chart: ChartData (7 days)
    
    Chart->>Syncfusion: UpdateDataSource(newData)
    Chart->>Syncfusion: Adjust axes ranges
    Chart->>Animation: Animate transition
    
    Chart->>Chart: Hide loading
    Chart->>Timer: Resume auto-refresh
    Chart-->>User: Chart with new period
    
    Note over User,Animation: Export Chart
    
    User->>Chart: Click export button
    Chart->>Chart: Show export options
    User->>Chart: Select "PNG"
    
    Chart->>Syncfusion: ExportAsync("image/png")
    Syncfusion->>Syncfusion: Render to canvas
    Syncfusion->>Syncfusion: Convert to blob
    Syncfusion-->>Chart: PNG blob
    
    Chart->>Chart: Create download link
    Chart->>Chart: Trigger download
    Chart-->>User: File downloaded
```

---

## UI Component Best Practices

### 1. Form Validation
- **Client-side first**: Validate immediately for better UX
- **Async validation**: Debounce server checks (300-500ms)
- **Clear errors**: Show/hide errors per field
- **Accessible**: Use ARIA attributes for screen readers
- **Progressive**: Validate on blur, then on input after first error

### 2. Data Grids
- **Optimistic updates**: Show changes immediately
- **Server-side operations**: Let API handle filtering/sorting/paging
- **Inline editing**: Validate before save
- **Keyboard navigation**: Support Tab, Enter, Escape
- **Loading states**: Show skeletons during data fetch

### 3. File Uploads
- **Drag & drop**: Support both click and drop
- **Preview generation**: Use FileReader for images
- **Progress tracking**: Show upload progress per file
- **Validation**: Check file type, size before upload
- **Error recovery**: Allow retry on failed uploads

### 4. Permissions
- **Cache aggressively**: Permissions rarely change (5min+ TTL)
- **Fail secure**: Hide/disable if permission check fails
- **Graceful degradation**: Show disabled state vs. hiding
- **Audit**: Log permission-based actions
- **Preload**: Fetch permissions on login

### 5. Theming
- **CSS variables**: Use for all theme properties
- **Smooth transitions**: Animate theme changes (300ms)
- **System preference**: Detect and respect OS theme
- **Persistence**: Save user choice to localStorage
- **Scope**: Support component-level theme overrides

### 6. Modals
- **Focus management**: Trap focus within modal
- **Keyboard support**: Close on Escape (if allowed)
- **Backdrop click**: Make configurable
- **Async support**: Handle long-running operations
- **Stacking**: Support multiple modals (z-index management)

### 7. Autocomplete
- **Debouncing**: Wait 300ms after typing stops
- **Min characters**: Require 2-3 chars before search
- **Caching**: Cache search results (5min TTL)
- **Keyboard navigation**: Arrow keys, Enter, Escape
- **Accessibility**: ARIA combobox pattern

### 8. Notifications
- **Auto-dismiss**: Default 3-5 seconds
- **Pause on hover**: Stop countdown when hovering
- **Stacking**: Limit to 3-5 toasts max
- **Priority**: Errors stay longer than success
- **Position**: Top-right or bottom-right (configurable)

### 9. Wizards
- **Save progress**: Store step data as you go
- **Validation per step**: Don't allow next if invalid
- **Navigation**: Allow back but validate forward
- **Summary**: Show review before final submit
- **Error handling**: Navigate to step with error

### 10. Charts
- **Real-time updates**: Use SignalR or polling
- **Smooth transitions**: Animate data changes
- **Responsive**: Adjust to container size
- **Export**: Support PNG, SVG, PDF export
- **Interaction**: Tooltips, zoom, pan

---

## Accessibility Checklist

All components must:
- ✅ Support keyboard navigation
- ✅ Include ARIA labels and roles
- ✅ Have sufficient color contrast (4.5:1)
- ✅ Work with screen readers
- ✅ Show focus indicators
- ✅ Provide error announcements
- ✅ Support high contrast mode
- ✅ Have touch-friendly targets (44x44px min)

---

**Document Version:** 1.0  
**Last Updated:** November 18, 2025  
**Maintained By:** Nalam360 Platform Team
