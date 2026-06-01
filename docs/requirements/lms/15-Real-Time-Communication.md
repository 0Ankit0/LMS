# 15. Real-Time Communication: Forums & Messaging

This document outlines the architecture for the real-time forum and private messaging systems, with a special focus on security and performance.

## 1. Core Technology

-   **Real-Time Layer**: All real-time functionality (live messages, forum posts, notifications) will be built using **SignalR**. A central `CommunicationHub` will manage connections and message broadcasting.
-   **End-to-End Encryption (E2EE)**: To ensure message privacy, the content of all private messages and forum posts will be end-to-end encrypted. 

    **⚠️ SECURITY WARNING:** Implementing E2EE is a highly complex and specialized task. This document describes the necessary architecture, but the actual implementation **must** use a well-vetted, battle-tested cryptographic library (such as a JavaScript implementation of the Signal Protocol, e.g., `libsignal-protocol-javascript`). **Do not attempt to create a custom E2EE protocol.**

## 2. Forum System

The forum is designed to be like a Discord or modern threaded forum.

-   **Data Model Clarification**: 
    -   `Forum`: A high-level category (e.g., "General Discussion", "Course Q&A").
    -   `ForumTopic`: This is the user-created "forum" with a title that other users interact with.
    -   `ForumPost`: An individual message within a `ForumTopic`.
-   **Real-Time Workflow**:
    1.  A user posts a message to a topic via `POST /api/forum-topics/{id}/posts`.
    2.  The API saves the post to the database. The `Content` is stored as **encrypted ciphertext**.
    3.  The API then invokes the SignalR Hub.
    4.  The Hub broadcasts the new (encrypted) post to all other clients currently subscribed to that `ForumTopic`.
    5.  The clients receive the message, decrypt it using their local keys, and display it in the UI.

## 3. Private & Group Messaging

To support messaging between two or more users, the data model needs to be updated from a simple from/to structure to one based on conversations.

-   **New Data Model**:
    -   `Conversation`: Represents a specific chat session.
    -   `ConversationParticipant`: A junction table linking `Users` to `Conversations`.
    -   `Message`: Now linked to a `ConversationId` instead of a `ToUserId`.
-   **Workflow: Starting a new Chat**:
    1.  User A selects User B and User C to start a group chat.
    2.  The client calls `POST /api/conversations` with the list of user IDs.
    3.  The backend creates a new `Conversation` and adds all three users as `ConversationParticipant`s.
-   **Workflow: Sending a Message**:
    1.  User A types a message in the new conversation.
    2.  On the client, the message is encrypted for User B and User C separately, using their public keys.
    3.  The client sends the encrypted content to `POST /api/messages`.
    4.  The API saves the message and invokes the SignalR Hub.
    5.  The Hub identifies all participants in the conversation and sends the appropriate encrypted message to each user's active connections.

## 4. E2EE Architectural Requirements

-   **Database**: The `Content` fields of the `Message` and `ForumPost` tables **do not store plaintext**. They store opaque, encrypted ciphertext.
-   **Server Role**: The server becomes a simple relay. It stores and forwards encrypted blobs of data but cannot read them. It has zero knowledge of the message content.
-   **Client Role**: The client is responsible for all cryptographic operations.
    -   **Key Management**: Each user must generate and store their own public/private key pair locally and securely. The private key must **never** be sent to the server.
    -   **Public Key Distribution**: The server's only role in keys is to store each user's public key, which other users can download to initiate a secure conversation.
    -   **Encryption/Decryption**: All messages are encrypted before being sent to the API and decrypted only when they are received on a recipient's client.
