# Workflow Editor Node Capabilities

This document outlines example workflows that can be built using the available nodes in the ZenFlow workflow editor.

## Available Node Categories & Types

The editor provides the following categories and types of nodes:

*   **Triggers:**
    *   `ZenFlow.Triggers.ManualTrigger`: Starts a workflow manually.
    *   `ZenFlow.Triggers.WebhookTrigger`: Starts a workflow via an HTTP call.
    *   `ZenFlow.Triggers.ScheduledTrigger`: Starts a workflow at a set time/interval.
*   **API:**
    *   `ZenFlow.Activities.Http.HttpRequestActivity`: Makes HTTP requests to external APIs.
*   **Data Operations:**
    *   `ZenFlow.Activities.Playwright.ExtractTextFromElementActivity`: Extracts text from web pages using CSS selectors.
    *   `ZenFlow.Activities.Variables.SetVariableActivity`: Stores a value in a variable for later use.
    *   `ZenFlow.Activities.Data.TransformDataActivity`: Modifies data structures or values (e.g., JSON, text).
*   **Control Flow:**
    *   `ZenFlow.Activities.ControlFlow.ConditionalActivity`: Executes different branches based on a condition.
    *   `ZenFlow.Activities.ControlFlow.DelayActivity`: Pauses workflow execution for a specified time.
    *   `ZenFlow.Activities.ControlFlow.LoopActivity`: Iterates over a list of items.
*   **Communication:**
    *   `ZenFlow.Activities.Email.SendEmailActivity`: Sends an email to one or more recipients.
*   **Browser Interaction:**
    *   `ZenFlow.Activities.Playwright.NavigateActivity`: Navigates to a URL.
    *   `ZenFlow.Activities.Playwright.ClickElementActivity`: Clicks on an element on the page.
    *   `ZenFlow.Activities.Playwright.InputTextActivity`: Types text into an input field.
    *   `ZenFlow.Activities.Playwright.WaitForSelectorActivity`: Waits for an element to appear on the page.
    *   `ZenFlow.Activities.Playwright.ScreenshotActivity`: Takes a screenshot of the page.

## Example Workflows

Here are some examples of workflows that can be created using the nodes listed above:

1.  **Daily Product Price Checker & Email Alert:**
    *   **Trigger:** `ScheduledTrigger` (e.g., daily).
    *   **Steps:**
        1.  `NavigateActivity` to a product page.
        2.  `WaitForSelectorActivity` for the price element.
        3.  `ExtractTextFromElementActivity` to get the price.
        4.  `SetVariableActivity` to store the price.
        5.  `TransformDataActivity` to clean/convert the price.
        6.  `ConditionalActivity` to check if the price is below a threshold.
        7.  If true, `SendEmailActivity` with a price alert.
        8.  (Optional) `ScreenshotActivity` of the product page.

2.  **New Blog Post Notifier:**
    *   **Trigger:** `ScheduledTrigger` (e.g., hourly).
    *   **Steps:**
        1.  `NavigateActivity` to a blog's main page.
        2.  `ExtractTextFromElementActivity` to get the latest post title/link.
        3.  `SetVariableActivity` to store the title.
        4.  `ConditionalActivity` to compare with a previously seen title (requires a mechanism to persist state between runs, e.g., updating a variable or using an external datastore via an HTTP request).
        5.  If new, `SendEmailActivity` with the new post details and update the "last seen post title".

3.  **Automated Form Submission from Webhook Data:**
    *   **Trigger:** `WebhookTrigger` (receives form data like name, email, message).
    *   **Steps:**
        1.  `SetVariableActivity` for each piece of received data (name, email, message).
        2.  `NavigateActivity` to a contact form page.
        3.  `InputTextActivity` to fill in the name field.
        4.  `InputTextActivity` to fill in the email field.
        5.  `InputTextActivity` to fill in the message field.
        6.  `ClickElementActivity` to submit the form.
        7.  (Optional) `SendEmailActivity` for admin confirmation.

4.  **Fetch API Data, Transform, and Email Report:**
    *   **Trigger:** `ManualTrigger` or `ScheduledTrigger`.
    *   **Steps:**
        1.  `HttpRequestActivity` to fetch data from an API (e.g., weather).
        2.  `SetVariableActivity` to store the API response.
        3.  `TransformDataActivity` to parse and format the relevant data.
        4.  `SendEmailActivity` with the formatted report.

5.  **Website Content Audit (Iterate through URLs):**
    *   **Trigger:** `ManualTrigger`.
    *   **Steps:**
        1.  `SetVariableActivity` to define a list of URLs.
        2.  `LoopActivity` to iterate through the URLs.
            *   `NavigateActivity` to the current URL.
            *   `WaitForSelectorActivity` for a specific keyword/element.
            *   `ExtractTextFromElementActivity` to get the element's text.
            *   `ConditionalActivity` to check if the text matches expectations.
            *   If not matching, `SendEmailActivity` with an alert.
        3.  (After Loop) `SendEmailActivity` with a summary report.

6.  **Basic Website Uptime Monitoring:**
    *   **Trigger:** `ScheduledTrigger` (e.g., every 30 minutes).
    *   **Steps:**
        1.  `NavigateActivity` to the website homepage.
        2.  `WaitForSelectorActivity` for a critical element.
        3.  `ConditionalActivity` to check if the element was found (or if navigation failed).
        4.  If an issue is detected, `SendEmailActivity` with an urgent alert.
        5.  (Optional) `ScreenshotActivity` if an error is detected.

This list demonstrates the versatility of the available nodes for creating various automation and data processing workflows.
