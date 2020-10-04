join : JOIN order (1) ON (1).customer_id = (0).customer_id
join : JOIN order (2) ON (2).customer_id = (0).customer_id
where : (1).order_id != (2).order_id -- ensure the primary keys for (1) and (2) are different.
