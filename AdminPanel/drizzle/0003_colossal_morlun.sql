CREATE TABLE `game_user_credentials` (
	`id` text PRIMARY KEY NOT NULL,
	`user_id` text NOT NULL,
	`salt` text NOT NULL,
	`hash` text NOT NULL,
	`iterations` integer DEFAULT 210000 NOT NULL,
	`updated_at` text DEFAULT CURRENT_TIMESTAMP NOT NULL,
	FOREIGN KEY (`user_id`) REFERENCES `game_users`(`id`) ON UPDATE no action ON DELETE cascade
);
--> statement-breakpoint
CREATE UNIQUE INDEX `game_user_credentials_user_id_unique` ON `game_user_credentials` (`user_id`);--> statement-breakpoint
CREATE INDEX `idx_game_user_credentials_user` ON `game_user_credentials` (`user_id`);