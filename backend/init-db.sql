-- PostgreSQL Initialization Script
-- This script runs when the PostgreSQL container starts for the first time
-- It creates the database schema extensions needed by the application

-- Enable UUID extension for better ID generation
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Enable pg_trgm for better text search (optional but production-ready)
CREATE EXTENSION IF NOT EXISTS "pg_trgm";

-- Grant permissions
GRANT ALL PRIVILEGES ON DATABASE disputeportal TO disputeportal;

-- Note: EF Core will handle table creation via migrations
-- This file just sets up the database extensions

-- Log successful initialization
DO $$
BEGIN
    RAISE NOTICE 'PostgreSQL database initialized successfully for Dispute Portal';
END $$;
